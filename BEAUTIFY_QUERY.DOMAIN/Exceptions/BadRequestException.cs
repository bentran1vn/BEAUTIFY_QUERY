namespace BEAUTIFY_QUERY.DOMAIN.Exceptions;
public abstract class BadRequestException : DomainException
{
    protected BadRequestException(string message)
        : base("Bad Request", message)
    {
    }
}