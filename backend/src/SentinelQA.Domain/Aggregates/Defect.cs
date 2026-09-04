using SentinelQA.Domain.Common;
using SentinelQA.Domain.Entities;
using SentinelQA.Domain.Enums;
using SentinelQA.Domain.Events;
using SentinelQA.Domain.Exceptions;

namespace SentinelQA.Domain.Aggregates;

public sealed class Defect : AggregateRoot<Guid>
{
    private static readonly Dictionary<DefectStatus, DefectStatus[]> AllowedTransitions = new()
    {
        [DefectStatus.Open]       = [DefectStatus.Triaged, DefectStatus.Closed],
        [DefectStatus.Triaged]    = [DefectStatus.InProgress, DefectStatus.Closed],
        [DefectStatus.InProgress] = [DefectStatus.Fixed, DefectStatus.Open],
        [DefectStatus.Fixed]      = [DefectStatus.Retest, DefectStatus.InProgress],
        [DefectStatus.Retest]     = [DefectStatus.Verified, DefectStatus.InProgress],
        [DefectStatus.Verified]   = [DefectStatus.Closed],
        [DefectStatus.Closed]     = []
    };

    private readonly List<DefectHistoryEntry> _history = [];

    public Guid TenantId { get; private set; }
    public string Title { get; private set; } = default!;
    public string Description { get; private set; } = default!;
    public Severity Severity { get; private set; }
    public PriorityLevel Priority { get; private set; }
    public DefectStatus Status { get; private set; }
    public string Environment { get; private set; } = default!;
    public string? Component { get; private set; }
    public string? StepsToReproduce { get; private set; }
    public string? ExpectedResult { get; private set; }
    public string? ActualResult { get; private set; }
    public Guid? TestRunId { get; private set; }
    public Guid? AssigneeId { get; private set; }
    public Guid? ReportedById { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public IReadOnlyCollection<DefectHistoryEntry> History => _history.AsReadOnly();

    private Defect() { } // EF Core

    public static Defect Report(
        Guid tenantId, string title, string description, Severity severity, PriorityLevel priority,
        string environment, Guid? reportedBy, Guid? testRunId, string? component = null,
        string? steps = null, string? expected = null, string? actual = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        var defect = new Defect
        {
            Id = Guid.CreateVersion7(),
            TenantId = tenantId,
            Title = title.Trim(),
            Description = description,
            Severity = severity,
            Priority = priority,
            Status = DefectStatus.Open,
            Environment = environment,
            Component = component,
            StepsToReproduce = steps,
            ExpectedResult = expected,
            ActualResult = actual,
            TestRunId = testRunId,
            ReportedById = reportedBy,
            CreatedAt = DateTimeOffset.UtcNow
        };

        defect.AddDomainEvent(new DefectCreated(defect.Id, tenantId, defect.Title, severity));
        return defect;
    }

    public void TransitionTo(DefectStatus target, Guid changedBy, string? note = null)
    {
        if (!AllowedTransitions[Status].Contains(target))
            throw new DomainException($"Invalid defect transition '{Status}' -> '{target}'.");

        var previous = Status;
        Status = target;

        if (target == DefectStatus.InProgress && AssigneeId is null)
            AssigneeId = changedBy;

        _history.Add(new DefectHistoryEntry(previous, target, changedBy, note));
        AddDomainEvent(new DefectStatusChanged(Id, TenantId, previous, target));
    }

    public void Assign(Guid assigneeId) => AssigneeId = assigneeId;
}