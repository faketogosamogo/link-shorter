using LinkShorter.Application.Options;
using LinkShorter.Core.Entities;
using LinkShorter.Core.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using FluentValidation;
using LinkShorter.Application.Exceptions;

namespace LinkShorter.Application.Features.ShortLinks.Commands;

public static class CreateShortLink
{
    public record Command(string Url) : IRequest<ShortLink>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.Url)
                .Must(c => Uri.IsWellFormedUriString(c, UriKind.Absolute))
                .WithMessage("Invalid Url");
        }
    }
    
    public class Handler : IRequestHandler<Command, ShortLink>
    {
        private readonly ILogger<Handler> _logger;

        private readonly IShortLinkRepository _shortLinkRepository;

        private readonly ShortLinkOptions _shortLinkOptions;
        
        public Handler(
            ILogger<Handler> logger,
            IShortLinkRepository shortLinkRepository,
            IOptions<ShortLinkOptions> shortLinkOptions)
        {
            _logger = logger;
            _shortLinkRepository = shortLinkRepository;
            _shortLinkOptions = shortLinkOptions.Value;
        }
        
        public async Task<ShortLink> Handle(Command command, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Started creating short link by url: {Url}", command.Url);

            var shortLinkWithSameUrl = await _shortLinkRepository.GetShortLinkByUrl(command.Url, cancellationToken);

            if (shortLinkWithSameUrl != null)
            {
                _logger.LogDebug("Short link by Url: {Url} was not created, returned short link: {ShortLinkId} with same Url ", command.Url, shortLinkWithSameUrl.Id);

                return shortLinkWithSameUrl;
            }
            
            var shortLink = new ShortLink(command.Url, _shortLinkOptions.TokenLength);

            var countOfTokenRegeneration = 0;
            
            // мб существует более продуманный алгоритм....
            while (await _shortLinkRepository.IsShortLinkByTokenExist(shortLink.Token, cancellationToken))
            {
                if (countOfTokenRegeneration > 2)
                {
                    var applicationEx = new InvalidTokenGenerationException("Count of token regeneration attempts was exceeded for Url: {0}", shortLink.Url);
                    
                    _logger.LogError(applicationEx, applicationEx.Message);

                    throw applicationEx;
                }
                
                shortLink.RegenerateToken(_shortLinkOptions.TokenLength);
                countOfTokenRegeneration++;
                
                _logger.LogDebug("Token regeneration for: {Url}", shortLink.Url);
            }

            _logger.LogDebug("Short link was created for url: {Url} with Token: {Token}", shortLink.Url, shortLink.Token);

            try
            {
                await _shortLinkRepository.AddShortLink(shortLink, cancellationToken);
                
                _logger.LogInformation("Short link for URL: {Url} was saved with Token: {Token} and Id: {ShortLinkId}", shortLink.Url, shortLink.Token, shortLink.Id);
            }
            catch (Exception ex)
            {
                var applicationEx = new ShortLinkSavingException(ex, "Saving short link with URL: {0} and Token: {1} error", shortLink.Url, shortLink.Token);
                
                _logger.LogError(applicationEx, applicationEx.Message);

                throw applicationEx;
            }

            return shortLink;
        }
    }
}