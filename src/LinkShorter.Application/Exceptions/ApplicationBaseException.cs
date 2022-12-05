namespace LinkShorter.Application.Exceptions;

public abstract class ApplicationBaseException : Exception
{
    protected ApplicationBaseException(string message, params object?[] args)
        : base(string.Format(message, args))
    {
    }
    protected ApplicationBaseException(Exception innerEx, string message, params object?[] args)
        : base(string.Format(message, args), innerEx)
    {
    }
}