using System.Net;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace PM.API.Middleware;

/// <summary>
/// Helper to write RFC 7807 compliant <see cref="ProblemDetails"/> responses for exceptions.
/// </summary>
public static class ProblemDetailsWriter
{
    /// <summary>
    /// Writes a <see cref="ProblemDetails"/> response based on the exception type.
    /// Maps common .NET exceptions to appropriate HTTP status codes and logs them.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    /// <param name="ex">The exception to map to a problem response.</param>
    /// <param name="logger">Logger used to record the exception.</param>
    public static async Task WriteAsync(HttpContext context, Exception ex, ILogger logger)
    {
        // Map exception types to HTTP status codes and titles
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

        // Log: server errors as error, client errors as info
        if ((int)status >= 500)
            logger.LogError(ex, "Unhandled exception");
        else
            logger.LogInformation(ex, "Handled exception: {Title}", title);

        var problem = new ProblemDetails
        {
            Type = "https://datatracker.ietf.org/doc/html/rfc7807",
            Title = title,
            Status = (int)status,
            Detail = ex.Message,
            Instance = context.Request.Path
        };

        // Include FluentValidation errors in the extensions dictionary
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
