using BEAUTIFY_QUERY.DOMAIN.Exceptions;

namespace BEAUTIFY_QUERY.APPLICATION.Exceptions;
public sealed class ValidationException : DomainException
{
    public ValidationException(IReadOnlyCollection<ValidationError> errors)
        : base("Validation Failure", "One or more validation errors occurred")
    {
        Errors = errors;
    }

    public IReadOnlyCollection<ValidationError> Errors { get; }
}

public record ValidationError(string PropertyName, string ErrorMessage);