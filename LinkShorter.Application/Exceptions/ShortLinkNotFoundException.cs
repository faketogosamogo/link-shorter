namespace LinkShorter.Application.Exceptions;

public class ShortLinkNotFoundException : ApplicationBaseException
{
    public ShortLinkNotFoundException(string message, params object?[] args) 
        : base(message, args)
    {
    }
}