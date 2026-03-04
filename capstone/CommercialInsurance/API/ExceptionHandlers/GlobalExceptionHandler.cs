// Provides core functionality and structures for the application.
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

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            _logger.LogError(exception, "Exception occurred: {Message}", exception.Message);

            var problemDetails = new ProblemDetails
            {
                Instance = httpContext.Request.Path,
                Title = "An error occurred",
                Detail = exception.InnerException != null
                    ? $"{exception.Message} (Inner: {exception.InnerException.Message}) \n {exception.StackTrace}"
                    : $"{exception.Message} \n {exception.StackTrace}"
            };

            switch (exception)
            {
                case ArgumentException:
                case InvalidOperationException:
                case ValidationException:
                case BusinessRuleException:
                    problemDetails.Status = (int)HttpStatusCode.BadRequest;
                    problemDetails.Title = "Bad Request";
                    break;
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
                default:
                    problemDetails.Status = (int)HttpStatusCode.InternalServerError;
                    problemDetails.Title = "Internal Server Error";
                    break;
            }

            httpContext.Response.StatusCode = problemDetails.Status.Value;

            await httpContext.Response
                .WriteAsJsonAsync(problemDetails, cancellationToken);

            return true;
        }
    }
}
