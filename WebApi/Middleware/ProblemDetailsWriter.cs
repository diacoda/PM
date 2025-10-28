using System.Net;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace PM.API.Middleware;

public static class ProblemDetailsWriter
{
    public static async Task WriteAsync(HttpContext context, Exception ex, ILogger logger)
    {
        var (status, title) = ex switch
        {
            ValidationException => (HttpStatusCode.BadRequest, "Validation failed"),
            ArgumentException or ArgumentOutOfRangeException => (HttpStatusCode.BadRequest, "Invalid argument"),
            KeyNotFoundException => (HttpStatusCode.NotFound, "Resource not found"),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "Unauthorized"),
            NotImplementedException => (HttpStatusCode.NotImplemented, "Not implemented"),
            InvalidOperationException => (HttpStatusCode.Conflict, "Invalid operation"),
            _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred")
        };

        // Log server errors with stack, client errors as info
        if ((int)status >= 500)
        {
            logger.LogError(ex, "Unhandled exception");
        }
        else
        {
            logger.LogInformation(ex, "Handled exception: {Title}", title);
        }

        var problem = new ProblemDetails
        {
            Type = "https://datatracker.ietf.org/doc/html/rfc7807",
            Title = title,
            Status = (int)status,
            Detail = ex.Message,
            Instance = context.Request.Path
        };

        // Map FluentValidation failures to a RFC7807 extension
        if (ex is ValidationException vex)
        {
            problem.Extensions["errors"] = vex.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray()
                );
        }

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = problem.Status ?? (int)status;
        await context.Response.WriteAsJsonAsync(problem);
    }
}
