using SentinelQA.Application.Abstractions;
using SentinelQA.Application.Abstractions.External;
using SentinelQA.Application.Abstractions.Infrastructure;
using SentinelQA.Application.Documents;
using SentinelQA.Contracts.Messages;

namespace SentinelQA.Modules.Testing.Workers;

internal sealed class AnalyzeFailureWithAiCommandHandler(
    IAssistantProvider assistant,
    IDocumentStore documents) : CommandHandlerBase<AnalyzeFailureWithAiCommand>
{
    public override async Task HandleAsync(AnalyzeFailureWithAiCommand command, CancellationToken cancellationToken)
    {
        var prompt = $"""
            You are a senior QA engineer. Analyze this automated test failure and respond with JSON:
            {{ "likelyRootCause": "...", "suggestedReproduction": "...", "suggestedSeverity": "...", "confidence": 0.0 }}

            Failure summary:
            {command.FailureSummary}
            """;

        var response = await assistant.GenerateAsync(prompt, cancellationToken);

        await documents.SaveAsync("aiAnalyses", new AiAnalysisDocument
        {
            TenantId = command.TenantId,
            AnalysisType = "failure-analysis",
            Input = command.FailureSummary,
            RawOutput = response.RawOutput,
            Provider = response.Provider,
            Confidence = response.Confidence
        }, cancellationToken);
    }
}