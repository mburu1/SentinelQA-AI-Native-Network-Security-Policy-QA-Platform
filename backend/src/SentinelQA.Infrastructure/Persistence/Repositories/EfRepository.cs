using Microsoft.EntityFrameworkCore;
using SentinelQA.Application.Abstractions.Persistence;
using SentinelQA.Domain.Common;

namespace SentinelQA.Infrastructure.Persistence.Repositories;

public class EfRepository<T>(ApplicationDbContext context) : IRepository<T, Guid>
    where T : AggregateRoot<Guid>
{
    protected ApplicationDbContext Context { get; } = context;

    public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await Context.Set<T>().FindAsync([id], cancellationToken);

    public async Task AddAsync(T entity, CancellationToken cancellationToken = default) =>
        await Context.Set<T>().AddAsync(entity, cancellationToken);

    public void Update(T entity) => Context.Set<T>().Update(entity);
    public void Remove(T entity) => Context.Set<T>().Remove(entity);
}