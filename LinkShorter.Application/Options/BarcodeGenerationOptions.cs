namespace LinkShorter.Application.Options;

public record BarcodeGenerationOptions(string BaseUrl)
{
    public BarcodeGenerationOptions() : this(string.Empty) {}
}