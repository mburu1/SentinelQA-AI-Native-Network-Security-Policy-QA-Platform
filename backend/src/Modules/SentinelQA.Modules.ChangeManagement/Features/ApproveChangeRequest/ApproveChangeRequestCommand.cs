using MediatR;

namespace SentinelQA.Modules.ChangeManagement.Features.ApproveChangeRequest;

public sealed record ApproveChangeRequestCommand(Guid ChangeRequestId) : IRequest<bool>;