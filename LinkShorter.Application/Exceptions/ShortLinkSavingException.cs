namespace LinkShorter.Application.Exceptions;

public class ShortLinkSavingException : ApplicationBaseException
{
    public ShortLinkSavingException(string message, params object?[] args) 
        : base(message, args)
    {
    }

    public ShortLinkSavingException(Exception innerEx, string message,  params object?[] args) 
        : base(innerEx, message, args)
    {
    }
}