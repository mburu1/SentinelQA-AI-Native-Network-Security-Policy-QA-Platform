using SentinelQA.Domain.Entities;

namespace SentinelQA.Modules.Policies.Abstractions;

public interface IValidationResultStore
{
    Task AddAsync(PolicyValidationResult result, CancellationToken cancellationToken);
}