namespace LinkShorter.Application.Services;

public interface IBarcodeGenerationService
{
    Stream GenerateAsImageStream(string url);
}