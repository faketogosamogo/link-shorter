using LinkShorter.Core.Entities;
using LinkShorter.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LinkShorter.Infrastructure.Repositories;

public class BarcodeInfoRepository : IBarcodeInfoRepository
{
    private readonly DatabaseContext _context;

    public BarcodeInfoRepository(
        DatabaseContext context)
    {
        _context = context;
    }

    public Task<BarcodeInfo?> GetBarcodeInfoByShortLinkId(long shortLinkId, CancellationToken cancellationToken) =>
        _context.BarcodeInfos.SingleOrDefaultAsync(c => c.ShortLinkId == shortLinkId, cancellationToken);

    public async Task AddBarcodeInfo(BarcodeInfo barcodeInfo, CancellationToken cancellationToken)
    {
        await _context.BarcodeInfos.AddAsync(barcodeInfo, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
}