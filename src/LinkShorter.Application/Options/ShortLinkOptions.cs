namespace LinkShorter.Application.Options;

public record ShortLinkOptions(int TokenLength)
{
    public ShortLinkOptions() : this(5) {}
}