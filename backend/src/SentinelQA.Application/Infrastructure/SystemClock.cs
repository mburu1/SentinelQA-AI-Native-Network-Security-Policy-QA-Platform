using SentinelQA.Application.Abstractions;

namespace SentinelQA.Application.Infrastructure;

public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}