using LinkShorter.Application.Exceptions;
using LinkShorter.Application.Options;
using LinkShorter.Application.Services;
using LinkShorter.Core.Entities;
using LinkShorter.Core.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LinkShorter.Application.Features.Barcode.Commands;

public static class GetOrCreateBarcodeByShortLinkToken
{
    public record Command(string ShortLinkToken) : IRequest<Stream>;

    public class Handler : IRequestHandler<Command, Stream>
    {
        private readonly ILogger<Handler> _logger;

        private readonly IShortLinkRepository _shortLinkRepository;

        private readonly IBarcodeInfoRepository _barcodeInfoRepository;
        
        private readonly IBarcodeGenerationService _barcodeGenerationService;

        private readonly IBarcodeStorageService _barcodeStorageService;

        private readonly BarcodeGenerationOptions _barcodeGenerationOptions;

        private readonly BarcodeStorageOptions _barcodeStorageOptions;

        public Handler(
            ILogger<Handler> logger,
            IShortLinkRepository shortLinkRepository,
            IBarcodeInfoRepository barcodeInfoRepository,
            IBarcodeGenerationService barcodeGenerationService,
            IBarcodeStorageService barcodeStorageService,
            IOptions<BarcodeGenerationOptions> barcodeGenerationOptions,
            IOptions<BarcodeStorageOptions> barcodeStorageOptions)
        {
            _logger = logger;
            _shortLinkRepository = shortLinkRepository;
            _barcodeInfoRepository = barcodeInfoRepository;
            _barcodeGenerationService = barcodeGenerationService;
            _barcodeStorageService = barcodeStorageService;
            _barcodeGenerationOptions = barcodeGenerationOptions.Value;
            _barcodeStorageOptions = barcodeStorageOptions.Value;
        }
        
        public async Task<Stream> Handle(Command command, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Получен запрос на создание/получение QR кода по токену: {Token}", command.ShortLinkToken);

            var shortLink = await _shortLinkRepository.GetShortLinkByToken(command.ShortLinkToken, cancellationToken);

            if (shortLink == null)
            {
                var applicationEx = new ShortLinkNotFoundException("При получении/создании QR кода для короткой ссылки по токену: {0} короткая ссылка не была найдена", command.ShortLinkToken);
                
                _logger.LogDebug(applicationEx, applicationEx.Message);

                throw applicationEx;
            }

            var barcodeInfo = await _barcodeInfoRepository.GetBarcodeInfoByShortLinkId(shortLink.Id, cancellationToken);

            Stream? barcodeReadStream = null;
            
            if (barcodeInfo == null)
            {
                var barcodeGenerationUrl = string.Format(_barcodeGenerationOptions.BaseUrl, shortLink.Token);

                try
                {
                    barcodeReadStream = _barcodeGenerationService.GenerateAsImageStream(barcodeGenerationUrl);
                    
                    _logger.LogDebug("При получении запроса на создание/получение QR кода по токену: {Token} был сгенерирован новый QR код", command.ShortLinkToken);
                }
                catch (Exception ex)
                {
                    if (barcodeReadStream != null)
                    {
                        await barcodeReadStream.DisposeAsync();   
                    }
                
                    var applicationEx = new BarcodeGenerationException(ex, "Ошибка генерации QR кода для Url: {0}", barcodeGenerationUrl);
                
                    _logger.LogError(applicationEx, applicationEx.Message);

                    throw applicationEx;
                }

                var barcodeSavePath = Path.Combine(_barcodeStorageOptions.BasePath, shortLink.Token);

                try
                {
                    await _barcodeStorageService.SaveBarcode(barcodeReadStream, barcodeSavePath, cancellationToken);
                    barcodeReadStream.Position = 0;
                    
                    _logger.LogDebug("При получении запроса на создание/получение QR кода по токену: {Token} был сохранен сгенерированый QR код", command.ShortLinkToken);
                }
                catch (Exception ex)
                {
                    await barcodeReadStream.DisposeAsync();

                    var applicationEx = new BarcodeSavingException(ex, "Ошибка cохранения QR кода для Url: {0}", barcodeGenerationUrl);
                
                    _logger.LogError(applicationEx, applicationEx.Message);

                    throw applicationEx;
                }
                
                barcodeInfo = new BarcodeInfo(barcodeSavePath, shortLink.Id);

                try
                {
                    await _barcodeInfoRepository.AddBarcodeInfo(barcodeInfo, cancellationToken);
                    
                    _logger.LogInformation("При получении запроса на создание/получение QR кода по токену: {Token} была сохранена информация о сохраненном и сгенерированном QR коде", command.ShortLinkToken);
                }
                catch (Exception ex)
                {
                    var applicationEx = new BarcodeInfoSavingException(ex, "Ошибка cохранения информации о QR коде для Url: {0}", barcodeGenerationUrl);
                
                    _logger.LogError(applicationEx, applicationEx.Message);

                    throw applicationEx;
                }
            }
            else
            {
                try
                {
                    barcodeReadStream = _barcodeStorageService.ReadBarcode(barcodeInfo.Path);
                    
                    _logger.LogDebug("При получении запроса на создание/получение QR кода по токену: {Token} был считан существующий Qr код", command.ShortLinkToken);
                }
                catch (Exception ex)
                {
                    var applicationEx = new BarcodeReadingException(ex, "Ошибка чтения QR кода по токену: {0}", shortLink.Token);
                    
                    _logger.LogError(applicationEx, applicationEx.Message);

                    throw applicationEx;
                }
            }

            return barcodeReadStream;
        }
    }
}