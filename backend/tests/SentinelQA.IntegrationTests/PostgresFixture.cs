using Microsoft.EntityFrameworkCore;
using SentinelQA.Infrastructure.Persistence;
using Testcontainers.PostgreSql;
using Xunit;

namespace SentinelQA.IntegrationTests;

public sealed class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:17-alpine")
        .WithDatabase("sentinelqa_tests")
        .Build();

    public string ConnectionString => _postgres.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        await using var context = CreateContext();
        await context.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync() => await _postgres.DisposeAsync();

    public ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        // MediatR + mapper are only needed for event dispatch; null-safe stubs keep tests focused.
        return new ApplicationDbContext(options, new NoOpMediator(), new NoOpEventMapper());
    }
}

internal sealed class NoOpMediator : MediatR.IMediator
{
    public IAsyncEnumerable<TResponse> CreateStream<TResponse>(MediatR.IStreamRequest<TResponse> request, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException();

    public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException();

    public Task Publish(object notification, CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task<TResponse> Send<TResponse>(MediatR.IRequest<TResponse> request, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException();

    public Task<object?> Send(object request, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException();

    public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : MediatR.IRequest =>
        Task.CompletedTask;
}

internal sealed class NoOpEventMapper : SentinelQA.Infrastructure.Persistence.Outbox.IntegrationEventMapper
{
    public NoOpEventMapper() : base(new NullCorrelation()) { }

    private sealed class NullCorrelation : SentinelQA.Infrastructure.Persistence.Outbox.ICorrelationContextAccessor
    {
        public Guid CorrelationId => Guid.Empty;
    }
}

[CollectionDefinition("postgres")]
public sealed class PostgresCollection : ICollectionFixture<PostgresFixture>;