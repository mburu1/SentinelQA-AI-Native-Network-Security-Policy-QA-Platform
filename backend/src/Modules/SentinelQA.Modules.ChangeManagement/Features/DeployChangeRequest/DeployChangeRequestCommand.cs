using MediatR;

namespace SentinelQA.Modules.ChangeManagement.Features.DeployChangeRequest;

public sealed record DeployChangeRequestCommand(Guid ChangeRequestId) : IRequest<bool>;