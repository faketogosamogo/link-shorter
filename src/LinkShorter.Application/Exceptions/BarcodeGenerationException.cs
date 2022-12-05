namespace LinkShorter.Application.Exceptions;

public class BarcodeGenerationException : ApplicationBaseException
{
    public BarcodeGenerationException(Exception innerException, string message, params object?[] args) 
        : base(innerException, message, args)
    {
    }
}