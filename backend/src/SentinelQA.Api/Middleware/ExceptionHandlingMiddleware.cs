using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SentinelQA.Application.Common;
using SentinelQA.Domain.Exceptions;
using SentinelQA.Infrastructure.Persistence.Outbox;

namespace SentinelQA.Api.Middleware;

public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            await HandleAsync(context, exception);
        }
    }

    private async Task HandleAsync(HttpContext context, Exception exception)
    {
        var correlationId = new AsyncLocalCorrelationContext().CorrelationId;

        var problem = exception switch
        {
            ValidationException validation => CreateValidationProblem(validation),
            NotFoundException => CreateProblem(StatusCodes.Status404NotFound, "Resource not found.", exception.Message),
            ConflictException => CreateProblem(StatusCodes.Status409Conflict, "Conflict.", exception.Message),
            DomainException => CreateProblem(StatusCodes.Status422UnprocessableEntity, "Business rule violation.", exception.Message),
            ForbiddenException => CreateProblem(StatusCodes.Status403Forbidden, "Forbidden.", exception.Message),
            UnauthorizedAccessException => CreateProblem(StatusCodes.Status401Unauthorized, "Unauthorized.", exception.Message),
            _ => CreateProblem(StatusCodes.Status500InternalServerError, "An unexpected error occurred.", null)
        };

        problem.Extensions["correlationId"] = correlationId;

        if (problem.Status >= StatusCodes.Status500InternalServerError)
            logger.LogError(exception, "Unhandled exception. CorrelationId={CorrelationId}", correlationId);
        else
            logger.LogWarning("Handled exception {ExceptionType}: {Message}. CorrelationId={CorrelationId}",
                exception.GetType().Name, exception.Message, correlationId);

        context.Response.StatusCode = problem.Status ?? StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
    }

    private static ProblemDetails CreateProblem(int status, string title, string? detail) => new()
    {
        Status = status,
        Title = title,
        Detail = detail
    };

    private static ValidationProblemDetails CreateValidationProblem(ValidationException exception)
    {
        var errors = exception.Errors
            .GroupBy(f => f.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(f => f.ErrorMessage).ToArray());

        return new ValidationProblemDetails(errors)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "One or more validation errors occurred."
        };
    }
}