using Application.Common.Exceptions;
using Domain.Core;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace API.Middlewares;

/// <summary>
/// Handles exceptions globally and returns appropriate HTTP responses.
/// </summary>
/// <param name="logger"></param>
/// <param name="hostEnvironment"></param>
public class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IHostEnvironment hostEnvironment
) : IExceptionHandler
{
    /// <summary>
    /// Tries to handle the exception and write an appropriate response.
    /// </summary>
    /// <param name="httpContext"></param>
    /// <param name="exception"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;

        LogException(exception, traceId);

        if (exception is ValidationException validationException)
        {
            var validationProblem = CreateValidationProblemDetails(httpContext, validationException);
            httpContext.Response.StatusCode = validationProblem.Status ?? StatusCodes.Status400BadRequest;
            await httpContext.Response.WriteAsJsonAsync(validationProblem, cancellationToken);
        }
        else
        {
            var problem = CreateProblemDetails(httpContext, exception, traceId);
            httpContext.Response.StatusCode = problem.Status ?? StatusCodes.Status500InternalServerError;
            await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
        }

        return true;
    }

    private ProblemDetails CreateProblemDetails(HttpContext httpContext, Exception exception, string? traceId)
    {
        var (statusCode, title, detail) = MapException(exception);

        var problem = new ProblemDetails
        {
            Title = title,
            Status = statusCode,
            Detail = detail,
            Instance = httpContext.Request.Path,
        };

        problem.Extensions["traceId"] = traceId;

        if (hostEnvironment.IsDevelopment())
        {
            problem.Extensions["exception"] = exception.GetType().Name;
            problem.Extensions["stackTrace"] = exception.StackTrace;
        }

        return problem;
    }

    private static ValidationProblemDetails CreateValidationProblemDetails(
        HttpContext httpContext,
        ValidationException validationException)
    {
        var errors = validationException.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray()
            );

        return new ValidationProblemDetails(errors)
        {
            Title = "Validation Error",
            Status = StatusCodes.Status400BadRequest,
            Detail = "One or more validation errors has occurred.",
            Instance = httpContext.Request.Path
        };
    }

    private static (int statusCode, string title, string detail) MapException(Exception exception)
    {
        return exception switch
        {
            MustChangePasswordException => (StatusCodes.Status403Forbidden, "Password Change Required", exception.Message),
            BadRequestException => (StatusCodes.Status400BadRequest, "Bad Request", exception.Message),
            KeyNotFoundException => (StatusCodes.Status404NotFound, "Not Found", exception.Message),
            UnauthorizedAccessException => (StatusCodes.Status403Forbidden, "Unauthorized", exception.Message),
            MissingTenantIdException => (StatusCodes.Status400BadRequest, "Missing Tenant ID", exception.Message),
            UnauthenticatedException => (StatusCodes.Status401Unauthorized, "Unauthenticated", exception.Message),
            TenantNotFoundException => (StatusCodes.Status404NotFound, "Tenant Not Found", exception.Message),
            TenantNotActiveException => (StatusCodes.Status403Forbidden, "Tenant Not Active", exception.Message),
            DomainException => (StatusCodes.Status400BadRequest, "Bad Request", exception.Message),
            NoAccessToTheProjectException => (StatusCodes.Status403Forbidden, "Forbidden", exception.Message),
            NotFoundException => (StatusCodes.Status404NotFound, "Not Found", exception.Message),
            _ => (StatusCodes.Status500InternalServerError, "Server Error", "An unexpected error occurred.")
        };
    }

    private void LogException(Exception exception, string? traceId)
    {
        if (exception is ValidationException
            or KeyNotFoundException
            or BadRequestException
            or UnauthorizedAccessException
            or MissingTenantIdException)
        {
            logger.LogWarning(exception, "Client error occurred. TraceId: {TraceId}", traceId);
        }
        else
        {
            logger.LogError(exception, "Unhandled server error. TraceId: {TraceId}", traceId);
        }
    }
}