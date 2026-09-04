using MediatR;

namespace SentinelQA.Modules.Testing.Features.StartTestRun;

public sealed record StartTestRunCommand(Guid TestSuiteId, string Environment) : IRequest<Guid>;