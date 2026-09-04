using FluentAssertions;
using SentinelQA.Domain.Exceptions;
using SentinelQA.Domain.ValueObjects;
using Xunit;

namespace SentinelQA.UnitTests.Domain.Rules;

public sealed class CidrBlockTests
{
    [Theory]
    [InlineData("10.0.0.0/24", true)]
    [InlineData("192.168.10.5/32", true)]
    [InlineData("10.0.0.0/33", false)]
    [InlineData("10.0.0.999/24", false)]
    [InlineData("not-a-cidr", false)]
    public void Parse_validates_input(string cidr, bool expectedValid)
    {
        var isValid = CidrBlock.TryParse(cidr, out _);
        isValid.Should().Be(expectedValid);
    }

    [Fact]
    public void Contains_detects_subnet_containment()
    {
        var broad = CidrBlock.Parse("10.0.0.0/16");
        var narrow = CidrBlock.Parse("10.0.10.0/24");

        broad.Contains(narrow).Should().BeTrue();
        narrow.Contains(broad).Should().BeFalse();
    }

    [Fact]
    public void Overlaps_detects_partial_intersection()
    {
        var a = CidrBlock.Parse("10.0.0.0/23");   // 10.0.0.0 - 10.0.1.255
        var b = CidrBlock.Parse("10.0.1.0/24");  // fully inside a

        a.Overlaps(b).Should().BeTrue();
    }

    [Fact]
    public void Any_matches_everything()
    {
        var any = CidrBlock.Parse("0.0.0.0/0");
        var host = CidrBlock.Parse("10.20.0.10/32");

        any.Contains(host).Should().BeTrue();
        any.IsAny.Should().BeTrue();
    }
}