using MediatR;
using SentinelQA.Application.Abstractions;
using SentinelQA.Application.Abstractions.Messaging;
using SentinelQA.Application.Abstractions.Persistence;
using SentinelQA.Application.Common;
using SentinelQA.Contracts.Messages;
using SentinelQA.Domain.Aggregates;

namespace SentinelQA.Modules.Testing.Features.StartTestRun;

internal sealed class StartTestRunCommandHandler(
    ITestSuiteRepository testSuites,
    IRepository<TestRun, Guid> testRuns,
    IUnitOfWork unitOfWork,
    ICommandPublisher commands,
    ICurrentUser currentUser,
    ICorrelationContext correlation) : IRequestHandler<StartTestRunCommand, Guid>
{
    public async Task<Guid> Handle(StartTestRunCommand request, CancellationToken cancellationToken)
    {
        var suite = await testSuites.GetByIdAsync(request.TestSuiteId, cancellationToken)
            ?? throw new NotFoundException($"Test suite '{request.TestSuiteId}' was not found.");

        var run = TestRun.Queue(currentUser.TenantId, suite.Id, request.Environment);

        await testRuns.AddAsync(run, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await commands.PublishAsync(new RunTestSuiteCommand(run.Id, suite.Id, request.Environment, currentUser.TenantId)
        {
            CorrelationId = correlation.CorrelationId
        }, cancellationToken);

        return run.Id;
    }
}