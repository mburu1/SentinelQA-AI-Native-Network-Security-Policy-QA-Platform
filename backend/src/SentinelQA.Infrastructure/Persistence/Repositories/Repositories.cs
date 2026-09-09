using Microsoft.EntityFrameworkCore;
using SentinelQA.Application.Abstractions.Persistence;
using SentinelQA.Domain.Aggregates;
using SentinelQA.Domain.Entities;

namespace SentinelQA.Infrastructure.Persistence.Repositories;

public sealed class PolicyRepository(ApplicationDbContext context)
    : EfRepository<Policy>(context), IPolicyRepository
{
    public IQueryable<Policy> Query() => Context.Policies.AsNoTracking();

    public Task<Policy?> GetWithRulesAsync(Guid id, CancellationToken cancellationToken = default) =>
        Context.Policies.Include(p => p.Rules).FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
}

public sealed class UserRepository(ApplicationDbContext context)
    : EfRepository<User>(context), IUserRepository
{
    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        Context.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

    public Task<User?> GetByRefreshTokenHashAsync(string hash, CancellationToken cancellationToken = default) =>
        Context.Users.FirstOrDefaultAsync(u => u.RefreshTokenHash == hash, cancellationToken);
}

public sealed class ChangeRequestRepository(ApplicationDbContext context)
    : EfRepository<ChangeRequest>(context), IChangeRequestRepository
{
    public IQueryable<ChangeRequest> Query() => Context.ChangeRequests.AsNoTracking();

    public Task<ChangeRequest?> GetByTestRunIdAsync(Guid testRunId, CancellationToken cancellationToken = default) =>
        Context.ChangeRequests.FirstOrDefaultAsync(cr => cr.TestRunId == testRunId, cancellationToken);
}

public sealed class TestRunRepository(ApplicationDbContext context)
    : EfRepository<TestRun>(context), ITestRunRepository
{
    public IQueryable<TestRun> Query() => Context.TestRuns.AsNoTracking();

    public Task<TestRun?> GetWithResultsAsync(Guid id, CancellationToken cancellationToken = default) =>
        Context.TestRuns.Include(r => r.Results).FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
}

public sealed class TestSuiteRepository(ApplicationDbContext context)
    : EfRepository<TestSuite>(context), ITestSuiteRepository
{
    public IQueryable<TestSuite> Query() => Context.TestSuites.AsNoTracking();

    public Task<TestSuite?> GetWithCasesAsync(Guid id, CancellationToken cancellationToken = default) =>
        Context.TestSuites.Include(s => s.Cases).FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
}

public sealed class UnitOfWork(ApplicationDbContext context) : Application.Abstractions.IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        context.SaveChangesAsync(cancellationToken);
}