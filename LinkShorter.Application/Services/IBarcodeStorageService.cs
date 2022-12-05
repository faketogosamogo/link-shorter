namespace LinkShorter.Application.Services;

public interface IBarcodeStorageService
{
    Task SaveBarcode(Stream readStream, string path, CancellationToken cancellationToken);

    Stream ReadBarcode(string path);
}