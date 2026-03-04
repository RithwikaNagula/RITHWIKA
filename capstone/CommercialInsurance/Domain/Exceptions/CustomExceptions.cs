// Provides core functionality and structures for the application.
namespace Domain.Exceptions
{
    public abstract class BaseException : Exception
    {
        protected BaseException(string message) : base(message) { }
    }

    public class NotFoundException : BaseException
    {
        public NotFoundException(string message) : base(message) { }
    }

    public class ValidationException : BaseException
    {
        public ValidationException(string message) : base(message) { }
    }

    public class UnauthorizedException : BaseException
    {
        public UnauthorizedException(string message) : base(message) { }
    }

    public class BusinessRuleException : BaseException
    {
        public BusinessRuleException(string message) : base(message) { }
    }
}
