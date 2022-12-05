namespace LinkShorter.Application.Options;

public record BarcodeStorageOptions(string BasePath)
{
    public BarcodeStorageOptions() : this(string.Empty) {}
}