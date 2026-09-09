using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SentinelQA.Domain.ValueObjects;

namespace SentinelQA.Infrastructure.Persistence;

public sealed class CidrBlockConverter : ValueConverter<CidrBlock, string>
{
    public CidrBlockConverter()
        : base(
            cidr => cidr.ToString(),
            value => new CidrBlock(value))
    {
    }
}

public sealed class PortRangeConverter : ValueConverter<PortRange, string>
{
    public PortRangeConverter()
        : base(
            range => range.ToString(),
            value => PortRange.Parse(value))
    {
    }
}