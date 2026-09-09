using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System.Collections.Generic;
using Testcontainers.PostgreSql;
using Xunit;

namespace SentinelQA.ApiTests;

public sealed class SentinelApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:17-alpine")
        .WithDatabase("sentinelqa_api_tests")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Postgres"] = _postgres.GetConnectionString(),
                ["Messaging:Enabled"] = "false", // brokers not needed for HTTP contract tests
                ["Ai:Provider"] = "Fake"
            });
        });
    }

    // FIX: xUnit v3 requires ValueTask instead of Task
    public async ValueTask InitializeAsync()
    {
        await _postgres.StartAsync();
    }

    // FIX: Removed explicit interface declaration and changed to ValueTask
    public async ValueTask DisposeAsync()
    {
        await _postgres.DisposeAsync();
    }
}