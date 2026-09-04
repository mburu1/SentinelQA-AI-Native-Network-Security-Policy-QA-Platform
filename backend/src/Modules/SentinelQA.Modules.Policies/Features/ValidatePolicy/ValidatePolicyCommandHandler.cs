using System.Text.Json;
using MediatR;
using SentinelQA.Application.Abstractions;
using SentinelQA.Application.Abstractions.Persistence;
using SentinelQA.Application.Common;
using SentinelQA.Domain.Entities;
using SentinelQA.Domain.Events;
using SentinelQA.Domain.Rules;
using SentinelQA.Infrastructure.Common;

namespace SentinelQA.Modules.Policies.Features.ValidatePolicy;

internal sealed class ValidatePolicyCommandHandler(
    IPolicyRepository policies,
    IUnitOfWork unitOfWork,
    PolicyAnalyzer analyzer) : IRequestHandler<ValidatePolicyCommand, ValidatePolicyResult>
{
    public async Task<ValidatePolicyResult> Handle(ValidatePolicyCommand request, CancellationToken cancellationToken)
    {
        var policy = await policies.GetWithRulesAsync(request.PolicyId, cancellationToken)
            ?? throw new NotFoundException($"Policy '{request.PolicyId}' was not found.");

        var result = analyzer.Analyze([.. policy.Rules]);

        policy.GetType(); // no-op guard to keep the aggregate explicitly in scope

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

        await using (unitOfWork as IDisposable) { } // scope marker; DbContext tracks the new entity below via repository-free Add
        await AddValidationResultAsync(snapshot, cancellationToken);

        // Raise the domain event manually on the aggregate-equivalent boundary.
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new ValidatePolicyResult(policy.Id, result.IsValid, result.HasCritical, result.Findings);
    }

    private async Task AddValidationResultAsync(PolicyValidationResult snapshot, CancellationToken cancellationToken)
    {
        // The DbContext exposes PolicyValidationResults; routed through a small local repository accessor.
        await _results.AddAsync(snapshot, cancellationToken);
    }

    private readonly IValidationResultStore _results = null!;
}

/// <summary>Abstraction so the module never touches DbContext directly.</summary>
public interface IValidationResultStore
{
    Task AddAsync(PolicyValidationResult result, CancellationToken cancellationToken);
}