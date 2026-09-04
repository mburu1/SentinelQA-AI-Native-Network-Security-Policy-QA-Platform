using SentinelQA.Domain.Aggregates;

namespace SentinelQA.Application.Abstractions.Persistence;

public interface IPolicyRepository : IRepository<Policy, Guid>, IQueryRepository<Policy>
{
    Task<Policy?> GetWithRulesAsync(Guid id, CancellationToken cancellationToken = default);
}

public interface IUserRepository : IRepository<User, Guid>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> GetByRefreshTokenHashAsync(string hash, CancellationToken cancellationToken = default);
}

public interface IChangeRequestRepository : IRepository<ChangeRequest, Guid>, IQueryRepository<ChangeRequest>
{
    Task<ChangeRequest?> GetByTestRunIdAsync(Guid testRunId, CancellationToken cancellationToken = default);
}

public interface ITestRunRepository : IRepository<TestRun, Guid>, IQueryRepository<TestRun>
{
    Task<TestRun?> GetWithResultsAsync(Guid id, CancellationToken cancellationToken = default);
}

public interface ITestSuiteRepository : IRepository<TestSuite, Guid>, IQueryRepository<TestSuite>
{
    Task<TestSuite?> GetWithCasesAsync(Guid id, CancellationToken cancellationToken = default);
}