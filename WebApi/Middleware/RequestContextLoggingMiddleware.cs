using Microsoft.Extensions.Primitives;
using Serilog.Context;

namespace PM.API.Middleware
{
    /// <summary>
    /// Middleware that enriches log context with a correlation ID for each HTTP request.
    /// </summary>
    /// <remarks>
    /// This middleware checks for a <c>Correlation-Id</c> header in the incoming request.
    /// If it exists, that value is used as the correlation ID; otherwise, the middleware
    /// falls back to using the request's <see cref="HttpContext.TraceIdentifier"/>.
    /// The correlation ID is then added to the Serilog log context so it can be included
    /// automatically in all log events for the request.
    /// </remarks>
    public class RequestContextLoggingMiddleware
    {
        private const string CorrelationIdHeaderName = "Correlation-Id";
        private readonly RequestDelegate _next;

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestContextLoggingMiddleware"/> class.
        /// </summary>
        /// <param name="next">The next middleware delegate in the request processing pipeline.</param>
        public RequestContextLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        /// <summary>
        /// Invokes the middleware for the current HTTP request.
        /// </summary>
        /// <param name="context">The current <see cref="HttpContext"/>.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <remarks>
        /// The correlation ID is pushed to the Serilog log context for the lifetime of this request.
        /// </remarks>
        public Task Invoke(HttpContext context)
        {
            using (LogContext.PushProperty("CorrelationId", GetCorrelationId(context)))
            {
                return _next.Invoke(context);
            }
        }

        /// <summary>
        /// Retrieves the correlation ID from the request header or generates one if not present.
        /// </summary>
        /// <param name="context">The current <see cref="HttpContext"/>.</param>
        /// <returns>
        /// The correlation ID from the request header if available; otherwise,
        /// the <see cref="HttpContext.TraceIdentifier"/>.
        /// </returns>
        private static string GetCorrelationId(HttpContext context)
        {
            context.Request.Headers.TryGetValue(CorrelationIdHeaderName, out StringValues correlationId);
            return correlationId.FirstOrDefault() ?? context.TraceIdentifier;
        }
    }
}
