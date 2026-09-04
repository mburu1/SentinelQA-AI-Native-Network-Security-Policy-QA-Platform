namespace SentinelQA.Application.Abstractions.Infrastructure;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);
    Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken cancellationToken = default);
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
}

public interface ILockService
{
    /// <summary>Returns a disposable lock handle, or null if the lock is already held.</summary>
    Task<IAsyncDisposable?> TryAcquireAsync(string key, TimeSpan ttl, CancellationToken cancellationToken = default);
}

public interface IIdempotencyStore
{
    /// <summary>Returns true when the key was registered for the first time.</summary>
    Task<bool> TryRegisterAsync(string key, TimeSpan ttl, CancellationToken cancellationToken = default);
}