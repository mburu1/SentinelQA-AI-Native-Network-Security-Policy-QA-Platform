using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace SentinelQA.Application.Behaviors;

public sealed class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var name = typeof(TRequest).Name;
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await next();
            logger.LogInformation("Handled {Request} in {ElapsedMs}ms", name, stopwatch.ElapsedMilliseconds);
            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to handle {Request} after {ElapsedMs}ms", name, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
}