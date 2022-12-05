namespace LinkShorter.Core.Utils;

public static class TokenGenerator
{
    private const string Alphabet = "1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    
    public static string Generate(int length)
    {
        if (length < 1)
        {
            throw new ArgumentException("Length must be more then 0");
        }
        
        var token = new char[length];
        var random = new Random();

        for (var i = 0; i < length; i++)
        {
            token[i] = Alphabet[random.Next(Alphabet.Length)];
        }

        return new string(token);
    }
}