using LinkShorter.Core.Entities;

namespace LinkShorter.Core.Repositories;

public interface IBarcodeInfoRepository
{
    Task<BarcodeInfo?> GetBarcodeInfoByShortLinkId(long shortLinkId, CancellationToken cancellationToken);

    Task AddBarcodeInfo(BarcodeInfo barcodeInfo, CancellationToken cancellationToken);
}