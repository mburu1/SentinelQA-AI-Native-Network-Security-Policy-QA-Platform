namespace SentinelQA.Application.Abstractions.External;

public interface IAssistantProvider
{
    string ProviderName { get; }
    Task<AssistantResponse> GenerateAsync(string prompt, CancellationToken cancellationToken = default);
}

public sealed record AssistantResponse(string RawOutput, string Provider, double Confidence);