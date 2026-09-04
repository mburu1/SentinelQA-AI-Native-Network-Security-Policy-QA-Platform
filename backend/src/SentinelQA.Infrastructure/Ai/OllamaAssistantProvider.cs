using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SentinelQA.Application.Abstractions.External;

namespace SentinelQA.Infrastructure.Ai;

public sealed class OllamaAssistantProvider(HttpClient http, IConfiguration configuration, ILogger<OllamaAssistantProvider> logger) : IAssistantProvider
{
    public string ProviderName => "Ollama";

    public async Task<AssistantResponse> GenerateAsync(string prompt, CancellationToken cancellationToken = default)
    {
        var baseUrl = configuration["Ollama:BaseUrl"] ?? "http://localhost:11434";
        var model = configuration["Ollama:Model"] ?? "qwen2.5:7b";

        var request = new
        {
            model,
            prompt,
            stream = false,
            format = "json"
        };

        var response = await http.PostAsJsonAsync($"{baseUrl}/api/generate", request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<OllamaResponse>(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Empty response from Ollama.");

        logger.LogDebug("Ollama generated {Length} characters.", payload.Response.Length);
        return new AssistantResponse(payload.Response, ProviderName, Confidence: 0.8);
    }

    private sealed record OllamaResponse(string Response);
}

/// <summary>Deterministic fallback used when Ollama is unavailable or disabled.</summary>
public sealed class FakeAssistantProvider : IAssistantProvider
{
    public string ProviderName => "Fake";

    public Task<AssistantResponse> GenerateAsync(string prompt, CancellationToken cancellationToken = default)
    {
        var output = JsonSerializer.Serialize(new
        {
            scenarios = new[]
            {
                new { title = "Public source with sensitive port is rejected", type = "negative", expected = "400/409 with CRITICAL finding" },
                new { title = "Internal SSH access is allowed when authorized", type = "positive", expected = "200 with valid policy" },
                new { title = "Single host /32 source passes CIDR validation", type = "boundary", expected = "200" },
                new { title = "Shadowed rule is detected", type = "security", expected = "analysis contains shadowedRule finding" }
            }
        });

        return Task.FromResult(new AssistantResponse(output, ProviderName, Confidence: 1.0));
    }
}