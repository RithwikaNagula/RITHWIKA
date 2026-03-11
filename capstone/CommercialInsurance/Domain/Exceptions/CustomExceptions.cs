// Domain-specific exception types (NotFoundException, ValidationException, UnauthorizedException) thrown by services and caught by the global handler.
namespace Domain.Exceptions
{
    // Abstract base for all domain exceptions; caught by GlobalExceptionHandler to produce structured error responses
    public abstract class BaseException : Exception
    {
        protected BaseException(string message) : base(message) { }
    }

    // Thrown when a requested entity does not exist; mapped to HTTP 404 Not Found
    public class NotFoundException : BaseException
    {
        public NotFoundException(string message) : base(message) { }
    }

    // Thrown when input data fails business rules (e.g., coverage amount out of range); mapped to HTTP 400 Bad Request
    public class ValidationException : BaseException
    {
        public ValidationException(string message) : base(message) { }
    }

    // Thrown when a user lacks permission for the requested action; mapped to HTTP 401 Unauthorized
    public class UnauthorizedException : BaseException
    {
        public UnauthorizedException(string message) : base(message) { }
    }

    // Thrown when an operation violates a domain business rule (e.g., policy already expired); mapped to HTTP 409 Conflict
    public class BusinessRuleException : BaseException
    {
        public BusinessRuleException(string message) : base(message) { }
    }
}
