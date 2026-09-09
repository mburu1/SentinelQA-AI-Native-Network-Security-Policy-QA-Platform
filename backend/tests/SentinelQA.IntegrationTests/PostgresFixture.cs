using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SentinelQA.Infrastructure.Persistence;
using SentinelQA.Infrastructure.Persistence.Outbox;
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

    // FIX 1: xUnit v3 requires ValueTask instead of Task for IAsyncLifetime
    public async ValueTask InitializeAsync()
    {
        await _postgres.StartAsync();
        await using var context = CreateContext();
        await context.Database.EnsureCreatedAsync();
    }

    // FIX 1: xUnit v3 requires ValueTask instead of Task for IAsyncDisposable
    public async ValueTask DisposeAsync()
    {
        await _postgres.DisposeAsync();
    }

    public ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        // FIX 2: IntegrationEventMapper is sealed, so we instantiate it directly 
        // instead of trying to inherit from it via NoOpEventMapper.
        var mapper = new IntegrationEventMapper(new NullCorrelationContextAccessor());

        return new ApplicationDbContext(options, new NoOpMediator(), mapper);
    }
}

// FIX 3: MediatR v12+ requires implementing IPublisher's generic Publish method
internal sealed class NoOpMediator : MediatR.IMediator
{
    public IAsyncEnumerable<TResponse> CreateStream<TResponse>(MediatR.IStreamRequest<TResponse> request, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException();

    public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException();

    public Task Publish(object notification, CancellationToken cancellationToken = default) => Task.CompletedTask;

    // Added missing generic Publish method required by IPublisher
    public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
        where TNotification : MediatR.INotification => Task.CompletedTask;

    public Task<TResponse> Send<TResponse>(MediatR.IRequest<TResponse> request, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException();

    public Task<object?> Send(object request, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException();

    public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : MediatR.IRequest =>
        Task.CompletedTask;
}

// Helper class to satisfy IntegrationEventMapper's constructor
internal sealed class NullCorrelationContextAccessor : ICorrelationContextAccessor
{
    public Guid CorrelationId => Guid.Empty;
}

[CollectionDefinition("postgres")]
public sealed class PostgresCollection : ICollectionFixture<PostgresFixture>;