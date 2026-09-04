using Microsoft.AspNetCore.Mvc;
using SentinelQA.Application.Abstractions.Infrastructure;

namespace SentinelQA.Api.Filters;

/// <summary>
/// Enforces idempotency for state-changing endpoints when the client supplies
/// an 'Idempotency-Key' header (spec §25). Duplicate submissions receive 409.
/// </summary>
public sealed class IdempotencyFilter(IIdempotencyStore idempotencyStore) : IAsyncActionFilter
{
    private const string HeaderName = "Idempotency-Key";

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (HttpMethods.IsPost(context.HttpContext.Request.Method)
            && context.HttpContext.Request.Headers.TryGetValue(HeaderName, out var keyValues)
            && Guid.TryParse(keyValues, out var key))
        {
            var idempotencyKey = $"{context.HttpContext.Request.Method}:{context.HttpContext.Request.Path}:{key}";
            var isFirstAttempt = await idempotencyStore.TryRegisterAsync(idempotencyKey, TimeSpan.FromMinutes(10));

            if (!isFirstAttempt)
            {
                context.Result = new ConflictObjectResult(new
                {
                    title = "Duplicate request.",
                    detail = "A request with this Idempotency-Key was already processed."
                });
                return;
            }
        }

        await next();
    }
}