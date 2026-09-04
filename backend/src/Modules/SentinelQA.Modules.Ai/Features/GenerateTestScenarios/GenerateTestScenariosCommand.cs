using MediatR;

namespace SentinelQA.Modules.Ai.Features.GenerateTestScenarios;

public sealed record GenerateTestScenariosCommand(string Requirement) : IRequest<GenerateTestScenariosResult>;

public sealed record GenerateTestScenariosResult(string Provider, double Confidence, IReadOnlyList<GeneratedScenario> Scenarios);

public sealed record GeneratedScenario(string Title, string Type, string ExpectedResult);