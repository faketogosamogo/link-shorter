using LinkShorter.Core.Entities;
using LinkShorter.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LinkShorter.Infrastructure.Repositories;

public class ShortLinkRepository : IShortLinkRepository
{
    private readonly DatabaseContext _context;

    public ShortLinkRepository(DatabaseContext context)
    {
        _context = context;
    }
    
    public async Task AddShortLink(ShortLink shortLink, CancellationToken cancellationToken)
    {
        await _context.ShortLinks.AddAsync(shortLink, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public Task<ShortLink?> GetShortLinkByToken(string token, CancellationToken cancellationToken) =>
        _context.ShortLinks.SingleOrDefaultAsync(c => c.Token == token, cancellationToken);

    public Task<ShortLink?> GetShortLinkByUrl(string url, CancellationToken cancellationToken) =>
        _context.ShortLinks.SingleOrDefaultAsync(c => c.Url == url, cancellationToken);

    public Task<bool> IsShortLinkByTokenExist(string token, CancellationToken cancellationToken) =>
        _context.ShortLinks.AnyAsync(c => c.Token == token, cancellationToken);
}