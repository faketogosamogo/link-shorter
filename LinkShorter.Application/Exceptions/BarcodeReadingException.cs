namespace LinkShorter.Application.Exceptions;

public class BarcodeReadingException : ApplicationBaseException
{
    public BarcodeReadingException(string message, params object?[] args) : base(message, args)
    {
    }

    public BarcodeReadingException(Exception innerEx, string message, params object?[] args) : base(innerEx, message, args)
    {
    }
}