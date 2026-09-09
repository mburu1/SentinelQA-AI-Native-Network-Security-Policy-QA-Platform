namespace SentinelQA.Application.Documents;

/// <summary>
/// MongoDB document storing AI analysis results for QA scenarios, failure analysis, and test generation.
/// </summary>
public sealed class AiAnalysisDocument
{
    public string Id { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
    public string AnalysisType { get; set; } = string.Empty;
    public string Input { get; set; } = string.Empty;
    public string RawOutput { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    public string? TraceId { get; set; }
    public string? Metadata { get; set; }
}
