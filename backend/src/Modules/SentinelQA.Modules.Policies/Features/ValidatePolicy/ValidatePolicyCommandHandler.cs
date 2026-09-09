using System.Text.Json;
using MediatR;
using SentinelQA.Application.Abstractions;
using SentinelQA.Application.Abstractions.Persistence;
using SentinelQA.Application.Common;
using SentinelQA.Domain.Entities;
using SentinelQA.Domain.Rules;
using SentinelQA.Infrastructure.Common;

namespace SentinelQA.Modules.Policies.Features.ValidatePolicy;

internal sealed class ValidatePolicyCommandHandler(
    IPolicyRepository policies,
    IUnitOfWork unitOfWork,
    IValidationResultStore results, // FIX 1: Properly inject the store
    PolicyAnalyzer analyzer) : IRequestHandler<ValidatePolicyCommand, ValidatePolicyResult>
{
    public async Task<ValidatePolicyResult> Handle(ValidatePolicyCommand request, CancellationToken cancellationToken)
    {
        var policy = await policies.GetWithRulesAsync(request.PolicyId, cancellationToken)
            ?? throw new NotFoundException($"Policy '{request.PolicyId}' was not found.");

        var result = analyzer.Analyze([.. policy.Rules]);

        if (result.IsValid)
            policy.RecordValidationPassed();

        // Persist a snapshot of the analysis for auditability.
        var snapshot = new PolicyValidationResult(
            policy.Id,
            policy.Version,
            result.IsValid,
            result.Findings.Count,
            result.CountBySeverity(FindingSeverity.Critical),
            JsonSerializer.Serialize(result.Findings, JsonDefaults.Options));

        // FIX 2: Use the injected store directly. 
        // No manual disposal of unitOfWork! The DI container handles it.
        await results.AddAsync(snapshot, cancellationToken);

        // SaveChangesAsync commits both the aggregate state change (RecordValidationPassed) 
        // and the new validation snapshot in a single transaction.
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new ValidatePolicyResult(policy.Id, result.IsValid, result.HasCritical, result.Findings);
    }
}

/// <summary>Abstraction so the module never touches DbContext directly.</summary>
public interface IValidationResultStore
{
    Task AddAsync(PolicyValidationResult result, CancellationToken cancellationToken);
}