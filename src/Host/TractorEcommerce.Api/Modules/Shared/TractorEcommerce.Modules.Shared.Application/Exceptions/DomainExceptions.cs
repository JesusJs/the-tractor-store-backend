namespace TractorEcommerce.Api.Modules.Shared.TractorEcommerce.Modules.Shared.Application.Exceptions
{
    public class DomainNotFoundException : Exception
    {
        public DomainNotFoundException(string msg) : base(msg) { }
    }

    public class DomainConflictException : Exception
    {
        public DomainConflictException(string msg) : base(msg) { }
    }

    public class DomainValidationException : Exception
    {
        public object Details { get; }
        public DomainValidationException(string msg, object details) : base(msg)
        {
            Details = details;
        }
    }
}
