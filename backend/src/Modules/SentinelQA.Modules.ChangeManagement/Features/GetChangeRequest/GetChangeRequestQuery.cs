using MediatR;

namespace SentinelQA.Modules.ChangeManagement.Features.GetChangeRequest;

public sealed record GetChangeRequestQuery(Guid ChangeRequestId) : IRequest<ChangeRequestDetails?>;

public sealed record ChangeRequestDetails(
    Guid Id, Guid PolicyId, Guid RequestedBy, string Reason, string State,
    Guid? ApproverId, Guid? TestRunId, string? DeploymentRef, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt);