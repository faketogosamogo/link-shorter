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
            _logger.LogDebug("Started getting/creating qr Code for short link with token: {Token}", command.ShortLinkToken);

            var shortLink = await _shortLinkRepository.GetShortLinkByToken(command.ShortLinkToken, cancellationToken);

            if (shortLink == null)
            {
                var applicationEx = new ShortLinkNotFoundException("Short link with token: {0} was not found", command.ShortLinkToken);
                
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
                    
                    _logger.LogDebug("Generated new QR code for short link: {ShortLinkId}", shortLink.Id);
                }
                catch (Exception ex)
                {
                    if (barcodeReadStream != null)
                    {
                        await barcodeReadStream.DisposeAsync();   
                    }
                
                    var applicationEx = new BarcodeGenerationException(ex, "QR code for short link: {0} generation error", shortLink.Id);
                
                    _logger.LogError(applicationEx, applicationEx.Message);

                    throw applicationEx;
                }

                var barcodeSavePath = Path.Combine(_barcodeStorageOptions.BasePath, shortLink.Token);

                try
                {
                    await _barcodeStorageService.SaveBarcode(barcodeReadStream, barcodeSavePath, cancellationToken);
                    barcodeReadStream.Position = 0;
                    
                    _logger.LogDebug("QR code was saved for short link: {ShortLinkId}", shortLink.Id);
                }
                catch (Exception ex)
                {
                    await barcodeReadStream.DisposeAsync();

                    var applicationEx = new BarcodeSavingException(ex, "QR code for short link: {0} saving error", shortLink.Id);
                
                    _logger.LogError(applicationEx, applicationEx.Message);

                    throw applicationEx;
                }
                
                barcodeInfo = new BarcodeInfo(barcodeSavePath, shortLink.Id);

                try
                {
                    await _barcodeInfoRepository.AddBarcodeInfo(barcodeInfo, cancellationToken);
                    
                    _logger.LogInformation("Barcode info for short link: {ShortLinkId} was saved", shortLink.Id);
                }
                catch (Exception ex)
                {
                    var applicationEx = new BarcodeInfoSavingException(ex, "Barcode info for short link: {0} saving error", shortLink.Id);
                
                    _logger.LogError(applicationEx, applicationEx.Message);

                    throw applicationEx;
                }
            }
            else
            {
                try
                {
                    barcodeReadStream = _barcodeStorageService.ReadBarcode(barcodeInfo.Path);
                    
                    _logger.LogDebug("Qr code was read for short link: {ShortLinkId}", shortLink.Id);
                }
                catch (Exception ex)
                {
                    var applicationEx = new BarcodeReadingException(ex, "Qr code for short link: {0} reading error", shortLink.Id);
                    
                    _logger.LogError(applicationEx, applicationEx.Message);

                    throw applicationEx;
                }
            }

            return barcodeReadStream;
        }
    }
}