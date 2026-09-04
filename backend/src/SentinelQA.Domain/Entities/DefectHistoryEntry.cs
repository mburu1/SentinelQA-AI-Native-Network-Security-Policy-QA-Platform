using SentinelQA.Domain.Common;
using SentinelQA.Domain.Enums;

namespace SentinelQA.Domain.Entities;

public sealed class DefectHistoryEntry : Entity<Guid>
{
    public DefectStatus From { get; private set; }
    public DefectStatus To { get; private set; }
    public Guid ChangedBy { get; private set; }
    public string? Note { get; private set; }
    public DateTimeOffset OccurredAt { get; private set; }

    private DefectHistoryEntry() { } // EF Core

    public DefectHistoryEntry(DefectStatus from, DefectStatus to, Guid changedBy, string? note)
    {
        Id = Guid.CreateVersion7();
        From = from;
        To = to;
        ChangedBy = changedBy;
        Note = note;
        OccurredAt = DateTimeOffset.UtcNow;
    }
}