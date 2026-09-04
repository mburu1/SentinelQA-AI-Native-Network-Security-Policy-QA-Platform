using MediatR;

namespace SentinelQA.Modules.Defects.Features.CreateDefect;

public sealed record CreateDefectCommand(
    string Title, string Description, string Severity, string Priority, string Environment,
    Guid? TestRunId, string? Component, string? Steps, string? Expected, string? Actual) : IRequest<Guid>;