namespace SentinelQA.Domain.Common;

public abstract class Entity<TId> where TId : notnull
{
    public TId Id { get; protected set; } = default!;

    public override bool Equals(object? obj) => obj is Entity<TId> entity && Id.Equals(entity.Id);
    public override int GetHashCode() => Id.GetHashCode();
}