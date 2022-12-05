using LinkShorter.Application.Services;

namespace LinkShorter.Infrastructure.Services;

public class BarcodeStorageService : IBarcodeStorageService
{
    public async Task SaveBarcode(Stream readStream, string path, CancellationToken cancellationToken)
    {
        await using var writeStream = File.Create(path);

        await readStream.CopyToAsync(writeStream, cancellationToken);
    }

    public Stream ReadBarcode(string path) =>
        File.OpenRead(path);
}