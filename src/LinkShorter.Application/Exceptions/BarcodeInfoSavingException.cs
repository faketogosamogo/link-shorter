namespace LinkShorter.Application.Exceptions;

public class BarcodeInfoSavingException : ApplicationBaseException
{
    public BarcodeInfoSavingException(string message, params object?[] args) : base(message, args)
    {
    }

    public BarcodeInfoSavingException(Exception innerEx, string message, params object?[] args) : base(innerEx, message, args)
    {
    }
}