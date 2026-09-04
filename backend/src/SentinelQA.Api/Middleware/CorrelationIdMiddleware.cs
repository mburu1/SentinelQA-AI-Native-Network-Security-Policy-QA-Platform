using SentinelQA.Infrastructure.Persistence.Outbox;

namespace SentinelQA.Api.Middleware;

public sealed class CorrelationIdMiddleware(RequestDelegate next)
{
    private const string HeaderName = "X-Correlation-ID";

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers.TryGetValue(HeaderName, out var incoming)
            && Guid.TryParse(incoming, out var parsed)
                ? parsed
                : Guid.CreateVersion7();

        using var _ = AsyncLocalCorrelationContext.Begin(correlationId);

        context.Response.OnStarting(() =>
        {
            context.Response.Headers[HeaderName] = correlationId.ToString();
            return Task.CompletedTask;
        });

        await next(context);
    }
}