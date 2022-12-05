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
            _logger.LogDebug("Получен запрос на создание сокращенной ссылки для URL: {Url}", command.Url);

            var shortLinkWithSameUrl = await _shortLinkRepository.GetShortLinkByUrl(command.Url, cancellationToken);

            if (shortLinkWithSameUrl != null)
            {
                _logger.LogDebug("При запросе на создание сокращенной ссылки для URL: {Url} была возвращена существующая сокращенная ссылка c токеном: {Token}", command.Url, shortLinkWithSameUrl.Token);

                return shortLinkWithSameUrl;
            }
            
            var shortLink = new ShortLink(command.Url, _shortLinkOptions.TokenLength);

            var countOfTokenRegeneration = 0;
            
            // мб существует более продуманный алгоритм....
            while (await _shortLinkRepository.IsShortLinkByTokenExist(shortLink.Token, cancellationToken))
            {
                if (countOfTokenRegeneration > 2)
                {
                    var applicationEx = new InvalidTokenGenerationException("При запросе на создание сокращенной ссылки для URL: {0} повторная генерация токена привысила допустимое количество попыток", command.Url);
                    
                    _logger.LogError(applicationEx, applicationEx.Message);

                    throw applicationEx;
                }
                
                shortLink.RegenerateToken(_shortLinkOptions.TokenLength);
                countOfTokenRegeneration++;
                
                _logger.LogDebug("При запросе на создание сокращенной ссылки для URL: {Url} произошла повторная генерация токена", command.Url);
            }

            _logger.LogDebug("При запросе на создание сокращенной ссылки для URL: {Url} была создана сокращенная ссылка c токеном: {Token}", command.Url, shortLink.Token);

            try
            {
                await _shortLinkRepository.AddShortLink(shortLink, cancellationToken);
                
                _logger.LogInformation("При запросе на создание сокращенной ссылки для URL: {Url} была сохранена сокращенная ссылка c токеном: {Token}", command.Url, shortLink.Token);
            }
            catch (Exception ex)
            {
                var applicationEx = new ShortLinkSavingException(ex, "Ошибка добавления созданной сокращенной ссылки для URL: {0}", command.Url);
                
                _logger.LogError(applicationEx, applicationEx.Message);

                throw applicationEx;
            }

            return shortLink;
        }
    }
}