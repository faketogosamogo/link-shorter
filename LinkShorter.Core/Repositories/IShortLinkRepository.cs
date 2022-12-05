using LinkShorter.Core.Entities;

namespace LinkShorter.Core.Repositories;

public interface IShortLinkRepository
{
    Task AddShortLink(ShortLink shortLink, CancellationToken cancellationToken);

    Task<ShortLink?> GetShortLinkByToken(string token, CancellationToken cancellationToken);

    Task<ShortLink?> GetShortLinkByUrl(string url, CancellationToken cancellationToken);
    
    Task<bool> IsShortLinkByTokenExist(string token, CancellationToken cancellationToken);
}