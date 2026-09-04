using MediatR;

namespace SentinelQA.Modules.ChangeManagement.Features.SubmitChangeRequest;

public sealed record SubmitChangeRequestCommand(Guid PolicyId, string Reason) : IRequest<Guid>;