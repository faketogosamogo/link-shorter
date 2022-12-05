using LinkShorter.Core.Utils;

namespace LinkShorter.Core.Entities;

public class ShortLink : BaseEntity
{
    public ShortLink(string url, int tokenLength)
    {
        if (tokenLength < 1)
        {
            throw new ArgumentException("TokenLength must be more then 0");
        }
        
        Token = TokenGenerator.Generate(tokenLength);
        Url = url;
    }

    // ef
    private ShortLink()
    {
    }
    
    public string Token { get; private set;  } = null!;

    /// <summary>
    /// Https is equal to Http. Maybe its not right.
    /// </summary>
    public string Url { get; } = null!;

    public void RegenerateToken(int tokenLength)
    {
        if (tokenLength < 1)
        {
            throw new ArgumentException("TokenLength must be more then 0");
        }
        
        Token = TokenGenerator.Generate(tokenLength);
    }
}