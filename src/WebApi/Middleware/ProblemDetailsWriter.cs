using System.Net;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using PM.SharedKernel;

namespace PM.API.Middleware
{
    /// <summary>
    /// Provides helper methods to write RFC 7807-compliant <see cref="ProblemDetails"/> responses
    /// that include domain <see cref="Error"/> information and correlation IDs.
    /// </summary>
    public static class ProblemDetailsWriter
    {
        /// <summary>
        /// Writes a <see cref="ProblemDetails"/> response for a given exception,
        /// mapping it to a <see cref="PM.SharedKernel.Error"/> and appropriate HTTP status code.
        /// </summary>
        /// <param name="context">The current <see cref="HttpContext"/>.</param>
        /// <param name="ex">The exception to map to a problem response.</param>
        /// <param name="logger">Logger used to record the exception.</param>
        public static async Task WriteAsync(HttpContext context, Exception ex, ILogger logger)
        {
            (Error error, HttpStatusCode status) = ex switch
            {
                ValidationException => (
                    new Error("Validation.Failed", "Validation failed", ErrorType.Validation),
                    HttpStatusCode.BadRequest),

                ArgumentException or ArgumentOutOfRangeException => (
                    new Error("Argument.Invalid", "Invalid argument", ErrorType.Validation),
                    HttpStatusCode.BadRequest),

                KeyNotFoundException => (
                    Error.NotFound("Resource.NotFound", "Requested resource not found"),
                    HttpStatusCode.NotFound),

                UnauthorizedAccessException => (
                    Error.Problem("Auth.Unauthorized", "Access is denied"),
                    HttpStatusCode.Unauthorized),

                NotImplementedException => (
                    Error.Problem("General.NotImplemented", "Feature not implemented"),
                    HttpStatusCode.NotImplemented),

                InvalidOperationException => (
                    Error.Conflict("Operation.Invalid", "Invalid operation attempted"),
                    HttpStatusCode.Conflict),

                _ => (
                    Error.Failure("Server.Error", "An unexpected error occurred"),
                    HttpStatusCode.InternalServerError)
            };

            if ((int)status >= 500)
                logger.LogError(ex, "{ErrorCode}: {ErrorDescription}", error.Code, error.Description);
            else
                logger.LogInformation(ex, "{ErrorCode}: {ErrorDescription}", error.Code, error.Description);

            // Retrieve correlation ID if available
            var correlationId = context.Request.Headers["Correlation-Id"].FirstOrDefault()
                                ?? context.TraceIdentifier;

            var problem = new ProblemDetails
            {
                Type = "https://datatracker.ietf.org/doc/html/rfc7807",
                Title = error.Description,
                Status = (int)status,
                Detail = ex.Message,
                Instance = context.Request.Path
            };

            problem.Extensions["error"] = new
            {
                code = error.Code,
                type = error.Type.ToString(),
                description = error.Description
            };

            // Add correlation ID
            problem.Extensions["correlationId"] = correlationId;

            if (ex is ValidationException vex)
            {
                problem.Extensions["validationErrors"] = vex.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray()
                    );
            }

            context.Response.ContentType = "application/problem+json";
            context.Response.StatusCode = problem.Status ?? (int)status;
            context.Response.Headers["Correlation-Id"] = correlationId;

            await context.Response.WriteAsJsonAsync(problem);
        }
    }
}
