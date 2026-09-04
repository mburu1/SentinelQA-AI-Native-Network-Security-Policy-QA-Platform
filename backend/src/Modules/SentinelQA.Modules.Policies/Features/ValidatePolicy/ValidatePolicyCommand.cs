using MediatR;
using SentinelQA.Domain.Rules;

namespace SentinelQA.Modules.Policies.Features.ValidatePolicy;

public sealed record ValidatePolicyCommand(Guid PolicyId) : IRequest<ValidatePolicyResult>;

public sealed record ValidatePolicyResult(Guid PolicyId, bool IsValid, bool HasCritical, IReadOnlyList<PolicyFinding> Findings);