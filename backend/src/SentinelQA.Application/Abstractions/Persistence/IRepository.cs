using SentinelQA.Domain.Common;

namespace SentinelQA.Application.Abstractions.Persistence;

public interface IRepository<T, in TId>
    where T : AggregateRoot<TId>
    where TId : notnull
{
    Task<T?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);
    Task AddAsync(T entity, CancellationToken cancellationToken = default);
    void Update(T entity);
    void Remove(T entity);
}

/// <summary>Read-model access for listing/filtering endpoints.</summary>
public interface IQueryRepository<T> where T : class
{
    IQueryable<T> Query();
}