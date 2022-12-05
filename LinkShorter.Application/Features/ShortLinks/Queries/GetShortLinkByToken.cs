using LinkShorter.Application.Exceptions;
using LinkShorter.Core.Entities;
using LinkShorter.Core.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LinkShorter.Application.Features.ShortLinks.Queries;

public static class GetShortLinkByToken
{
    public record Query(string Token) : IRequest<ShortLink>;

    public class Handler : IRequestHandler<Query, ShortLink>
    {
        private readonly ILogger<Handler> _logger;

        private readonly IShortLinkRepository _shortLinkRepository;
        
        public Handler(
            ILogger<Handler> logger,
            IShortLinkRepository shortLinkRepository)
        {
            _logger = logger;
            _shortLinkRepository = shortLinkRepository;
        }
        
        public async Task<ShortLink> Handle(Query query, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Получен запрос на получение сокращенной ссылки по токену: {Token}", query.Token);
            
            var shortLink = await _shortLinkRepository.GetShortLinkByToken(query.Token, cancellationToken);

            if (shortLink == null)
            {
                var applicationEx = new ShortLinkNotFoundException("Сокращенная ссылка не была найдена при получении запроса на получение сокращенной ссылки по токену: {0}", query.Token);
                
                _logger.LogDebug(applicationEx, applicationEx.Message);

                throw applicationEx;
            }

            _logger.LogDebug("Сокращенная ссылка была найдена при получении запроса на получение сокращенной ссылки по токену: {Token}", query.Token);

            return shortLink;
        }
    }
}