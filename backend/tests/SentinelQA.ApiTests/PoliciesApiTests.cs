using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace SentinelQA.ApiTests;

public sealed class PoliciesApiTests(SentinelApiFactory factory) : IClassFixture<SentinelApiFactory>
{
    [Fact]
    public async Task Health_live_returns_ok_without_infrastructure()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/health/live");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Unauthenticated_policy_request_returns_401()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/v1/policies");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Correlation_id_is_returned_on_every_response()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/health/live");

        response.Headers.Contains("X-Correlation-ID").Should().BeTrue();
    }
}