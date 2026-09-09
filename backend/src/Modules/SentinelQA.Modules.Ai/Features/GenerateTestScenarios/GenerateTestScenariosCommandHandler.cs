using System.Text.Json;
using MediatR;
using SentinelQA.Application.Abstractions;
using SentinelQA.Application.Abstractions.External;
using SentinelQA.Application.Abstractions.Infrastructure;
using SentinelQA.Application.Documents;

namespace SentinelQA.Modules.Ai.Features.GenerateTestScenarios;

internal sealed class GenerateTestScenariosCommandHandler(
    IAssistantProvider assistant,
    IDocumentStore documents,
    ICurrentUser currentUser) : IRequestHandler<GenerateTestScenariosCommand, GenerateTestScenariosResult>
{
    public async Task<GenerateTestScenariosResult> Handle(GenerateTestScenariosCommand request, CancellationToken cancellationToken)
    {
        // $$""" allows single '{' and '}' to be literal text (perfect for JSON).
        // We use '{{' and '}}' to interpolate the request.Requirement variable.
        var prompt = $$"""
            You are a senior network-security QA engineer. Generate test scenarios for this requirement
            as JSON: { "scenarios": [ { "title": "...", "type": "positive|negative|boundary|security", "expected": "..." } ] }
            
            Requirement:
            {{request.Requirement}}
            """;

        var response = await assistant.GenerateAsync(prompt, cancellationToken);

        await documents.SaveAsync("aiAnalyses", new AiAnalysisDocument
        {
            TenantId = currentUser.TenantId,
            AnalysisType = "test-scenarios",
            Input = request.Requirement,
            RawOutput = response.RawOutput,
            Provider = response.Provider,
            Confidence = response.Confidence
        }, cancellationToken);

        var scenarios = ParseScenarios(response.RawOutput);
        return new GenerateTestScenariosResult(response.Provider, response.Confidence, scenarios);
    }

    private static IReadOnlyList<GeneratedScenario> ParseScenarios(string rawOutput)
    {
        try
        {
            using var document = JsonDocument.Parse(rawOutput);
            return document.RootElement
                .GetProperty("scenarios")
                .EnumerateArray()
                .Select(s => new GeneratedScenario(
                    s.GetProperty("title").GetString() ?? "Untitled",
                    s.GetProperty("type").GetString() ?? "positive",
                    s.TryGetProperty("expected", out var expected) ? expected.GetString() ?? "" : ""))
                .ToList();
        }
        catch (JsonException)
        {
            // AI output is never trusted blindly; return empty when unparseable.
            return [];
        }
    }
}