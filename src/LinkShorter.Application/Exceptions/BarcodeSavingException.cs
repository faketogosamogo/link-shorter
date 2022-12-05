namespace LinkShorter.Application.Exceptions;

public class BarcodeSavingException : ApplicationBaseException
{
    public BarcodeSavingException(Exception innerEx, string message,  params object?[] args) 
        : base(innerEx, message, args)
    {
    }
}