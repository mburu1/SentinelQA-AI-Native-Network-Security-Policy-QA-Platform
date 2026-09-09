using System;
using MediatR;
using SentinelQA.Domain.Common;

namespace SentinelQA.Domain.Events;

public abstract record DomainEvent : IDomainEvent, INotification
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTimeOffset OccurredOn { get; init; } = DateTimeOffset.UtcNow;
}