using MediatR;

namespace SentinelQA.Modules.Defects.Features.TransitionDefect;

public sealed record TransitionDefectCommand(Guid DefectId, string TargetStatus, string? Note) : IRequest<bool>;