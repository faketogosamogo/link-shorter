namespace LinkShorter.Application.Exceptions;

public class InvalidTokenGenerationException : ApplicationBaseException
{
    public InvalidTokenGenerationException(string message, params object?[] args) 
        : base(message, args)
    {
    }
}