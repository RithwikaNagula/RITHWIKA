// Centralized exception handler that converts unhandled exceptions into structured RFC 7807 ProblemDetails responses.
using Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace API.ExceptionHandlers
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        // Intercepts any unhandled exception in the application pipeline and processes it.
        // Returns true if the exception was successfully handled, preventing it from propagating further.
        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            // Log the raw exception details to the server console/logs for debugging purposes
            _logger.LogError(exception, "Exception occurred: {Message}", exception.Message);

            var problemDetails = new ProblemDetails
            {
                Instance = httpContext.Request.Path,
                Title = "An error occurred",
                Detail = exception.InnerException != null
                    ? $"{exception.Message} (Inner: {exception.InnerException.Message})"
                    : $"{exception.Message}"
            };

            // Map standard application exceptions to their corresponding HTTP status codes
            switch (exception)
            {
                // Validation and business rules exceptions naturally map to a 400 Bad Request
                case ArgumentException:
                case InvalidOperationException:
                case ValidationException:
                case BusinessRuleException:
                    problemDetails.Status = (int)HttpStatusCode.BadRequest;
                    problemDetails.Title = "Bad Request";
                    break;
                // Security and access-control errors map to 401 Unauthorized
                case UnauthorizedException:
                case UnauthorizedAccessException:
                    problemDetails.Status = (int)HttpStatusCode.Unauthorized;
                    problemDetails.Title = "Unauthorized";
                    break;
                case NotFoundException:
                case KeyNotFoundException:
                    problemDetails.Status = (int)HttpStatusCode.NotFound;
                    problemDetails.Title = "Resource Not Found";
                    break;
                // Any other unhandled exception (e.g., NullReference, SQL crashes) defaults to 500
                default:
                    problemDetails.Status = (int)HttpStatusCode.InternalServerError;
                    problemDetails.Title = "Internal Server Error";
                    break;
            }

            // Apply the resolved status code directly onto the HTTP Context response stream
            httpContext.Response.StatusCode = problemDetails.Status.Value;

            // Serialize the structured ProblemDetails object directly to the client as JSON
            await httpContext.Response
                .WriteAsJsonAsync(problemDetails, cancellationToken);

            return true;
        }
    }
}
