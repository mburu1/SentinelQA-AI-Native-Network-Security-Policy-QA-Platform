using System.Net;
using Microsoft.Extensions.Configuration;
using SentinelQA.Application.Abstractions.Infrastructure;
using StackExchange.Redis;

namespace SentinelQA.Infrastructure.Redis;

public sealed class RedisCacheService(IConnectionMultiplexer redis) : ICacheService
{
    private IDatabase Db => redis.GetDatabase();

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var value = await Db.StringGetAsync(key);
        return value.HasValue
            ? System.Text.Json.JsonSerializer.Deserialize<T>((string)value!)
            : default;
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken cancellationToken = default) =>
        await Db.StringSetAsync(key, System.Text.Json.JsonSerializer.Serialize(value), ttl);

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default) =>
        await Db.KeyDeleteAsync(key);
}

public sealed class RedisLockService(IConnectionMultiplexer redis) : ILockService
{
    public async Task<IAsyncDisposable?> TryAcquireAsync(string key, TimeSpan ttl, CancellationToken cancellationToken = default)
    {
        var token = Guid.NewGuid().ToString();
        var db = redis.GetDatabase();

        var acquired = await db.LockTakeAsync(key, token, ttl);
        return acquired ? new LockHandle(db, key, token) : null;
    }

    private sealed class LockHandle(IDatabase db, string key, string token) : IAsyncDisposable
    {
        public async ValueTask DisposeAsync() => await db.LockReleaseAsync(key, token);
    }
}

public sealed class RedisIdempotencyStore(IConnectionMultiplexer redis) : IIdempotencyStore
{
    public async Task<bool> TryRegisterAsync(string key, TimeSpan ttl, CancellationToken cancellationToken = default)
    {
        var db = redis.GetDatabase();
        return await db.StringSetAsync($"idempotency:{key}", "1", ttl, When.NotExists);
    }
}