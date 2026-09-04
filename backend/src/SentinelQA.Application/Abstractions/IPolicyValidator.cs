namespace SentinelQA.Application.Abstractions;

public interface IPolicyValidator
{
    Task<PolicyValidationResult> ValidateAsync(
        Guid firewallId,
        CancellationToken cancellationToken = default);
}

public sealed record PolicyValidationResult(
    bool IsValid,
    IReadOnlyCollection<string> Errors);
