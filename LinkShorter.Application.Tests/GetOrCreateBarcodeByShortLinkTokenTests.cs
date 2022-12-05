using FluentAssertions;
using LinkShorter.Application.Exceptions;
using LinkShorter.Application.Features.Barcode.Commands;
using LinkShorter.Application.Features.ShortLinks.Commands;
using LinkShorter.Application.Options;
using LinkShorter.Application.Services;
using LinkShorter.Core.Entities;
using LinkShorter.Core.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace LinkShorter.Application.Tests;

[TestFixture]
public class GetOrCreateBarcodeByShortLinkTokenTests
{
    private ILogger<GetOrCreateBarcodeByShortLinkToken.Handler> _logger = new Mock<ILogger<GetOrCreateBarcodeByShortLinkToken.Handler>>().Object;
        
    private Randomizer _rnd = new Randomizer();

    [Test]
    public async Task Handle_Should_CreateNewBarcode()
    {
        // arange
        var bufferSize = 5;
        var buffer = Enumerable.Repeat(_rnd.NextByte(), bufferSize);
        await using var barcodeStream = new MemoryStream(buffer.ToArray());
        
        var shortLink = new ShortLink(_rnd.GetString(), 5)
        {
            Id = _rnd.Next()
        };

        var barcodeGenerationOptionsMock = new Mock<IOptions<BarcodeGenerationOptions>>();
        var barcodeStorageOptionsMock = new Mock<IOptions<BarcodeStorageOptions>>();
        var shortLinkRepositoryMock = new Mock<IShortLinkRepository>();
        var barcodeInfoRepositoryMock = new Mock<IBarcodeInfoRepository>();
        var barcodeGenerationServiceMock = new Mock<IBarcodeGenerationService>();
        
        barcodeGenerationOptionsMock
            .Setup(c => c.Value)
            .Returns(new BarcodeGenerationOptions(_rnd.GetString()));
        
        barcodeStorageOptionsMock
            .Setup(c => c.Value)
            .Returns(new BarcodeStorageOptions(_rnd.GetString()));
        
        shortLinkRepositoryMock
            .Setup(c => c.GetShortLinkByToken(shortLink.Token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(shortLink);

        barcodeInfoRepositoryMock
            .Setup(c => c.GetBarcodeInfoByShortLinkId(shortLink.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((BarcodeInfo?)null);
        
        barcodeGenerationServiceMock
            .Setup(c => c.GenerateAsImageStream(string.Format(barcodeGenerationOptionsMock.Object.Value.BaseUrl, shortLink.Token)))
            .Returns(barcodeStream);
        
        var barcodeStorageServiceMock = new Mock<IBarcodeStorageService>();

        var sut = new GetOrCreateBarcodeByShortLinkToken.Handler(
            _logger,
            shortLinkRepositoryMock.Object,
            barcodeInfoRepositoryMock.Object,
            barcodeGenerationServiceMock.Object,
            barcodeStorageServiceMock.Object,
            barcodeGenerationOptionsMock.Object,
            barcodeStorageOptionsMock.Object);

        // act
        var result = await sut.Handle(new GetOrCreateBarcodeByShortLinkToken.Command(shortLink.Token), CancellationToken.None);
        
        // assert
        result
            .Should()
            .BeReadable();
        
        barcodeInfoRepositoryMock
            .Verify(
                c => 
                    c.AddBarcodeInfo(
                        It.Is<BarcodeInfo>(
                            x => 
                                x.ShortLinkId == shortLink.Id &&
                                x.Path == Path.Combine(barcodeStorageOptionsMock.Object.Value.BasePath, shortLink.Token)),
                        It.IsAny<CancellationToken>()),
                Times.Once);
        
        barcodeStorageServiceMock
            .Verify(
                c => 
                    c.SaveBarcode(
                        barcodeStream, 
                        Path.Combine(barcodeStorageOptionsMock.Object.Value.BasePath, shortLink.Token),
                        It.IsAny<CancellationToken>()),
                Times.Once);
    }
    
    [Test]
    public async Task Handle_Should_ReturnExistBarcode()
    {
        // arange
        var bufferSize = 5;
        var buffer = Enumerable.Repeat(_rnd.NextByte(), bufferSize);
        
        await using var barcodeStream = new MemoryStream(buffer.ToArray());
        
        var shortLink = new ShortLink(_rnd.GetString(), 5)
        {
            Id = _rnd.Next()
        };
        
        var barcodeInfo = new BarcodeInfo(_rnd.GetString(), shortLink.Id);
        
        var barcodeGenerationOptionsMock = new Mock<IOptions<BarcodeGenerationOptions>>();
        var barcodeStorageOptionsMock = new Mock<IOptions<BarcodeStorageOptions>>();
        var shortLinkRepositoryMock = new Mock<IShortLinkRepository>();
        var barcodeInfoRepositoryMock = new Mock<IBarcodeInfoRepository>();
        var barcodeGenerationServiceMock = new Mock<IBarcodeGenerationService>();
        var barcodeStorageServiceMock = new Mock<IBarcodeStorageService>();

        barcodeGenerationOptionsMock
            .Setup(c => c.Value)
            .Returns(new BarcodeGenerationOptions(_rnd.GetString()));
        
        barcodeStorageOptionsMock
            .Setup(c => c.Value)
            .Returns(new BarcodeStorageOptions(_rnd.GetString()));
        
        shortLinkRepositoryMock
            .Setup(c => c.GetShortLinkByToken(shortLink.Token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(shortLink);
        
        barcodeInfoRepositoryMock
            .Setup(c => c.GetBarcodeInfoByShortLinkId(shortLink.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(barcodeInfo);
        
        barcodeStorageServiceMock
            .Setup(c => c.ReadBarcode(barcodeInfo.Path))
            .Returns(barcodeStream);
        
        var sut = new GetOrCreateBarcodeByShortLinkToken.Handler(
            _logger,
            shortLinkRepositoryMock.Object,
            barcodeInfoRepositoryMock.Object,
            barcodeGenerationServiceMock.Object,
            barcodeStorageServiceMock.Object,
            barcodeGenerationOptionsMock.Object,
            barcodeStorageOptionsMock.Object);

        // act
        var result = await sut.Handle(new GetOrCreateBarcodeByShortLinkToken.Command(shortLink.Token), CancellationToken.None);
        
        // assert
        result
            .Should()
            .BeReadable();
    }
    
    [Test]
    public async Task Handle_Should_ThrowShortLinkNotFoundException()
    {
        // arange
        var token = _rnd.GetString();
        
        var barcodeGenerationOptionsMock = new Mock<IOptions<BarcodeGenerationOptions>>();
        var barcodeStorageOptionsMock = new Mock<IOptions<BarcodeStorageOptions>>();
        var shortLinkRepositoryMock = new Mock<IShortLinkRepository>();
        var barcodeInfoRepositoryMock = new Mock<IBarcodeInfoRepository>();
        var barcodeGenerationServiceMock = new Mock<IBarcodeGenerationService>();
        var barcodeStorageServiceMock = new Mock<IBarcodeStorageService>();

        shortLinkRepositoryMock
            .Setup(c => c.GetShortLinkByToken(token, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ShortLink?)null);
        
        var sut = new GetOrCreateBarcodeByShortLinkToken.Handler(
            _logger,
            shortLinkRepositoryMock.Object,
            barcodeInfoRepositoryMock.Object,
            barcodeGenerationServiceMock.Object,
            barcodeStorageServiceMock.Object,
            barcodeGenerationOptionsMock.Object,
            barcodeStorageOptionsMock.Object);

        // act
        Func<Task> result = () => sut.Handle(new GetOrCreateBarcodeByShortLinkToken.Command(token), CancellationToken.None);
        
        // assert
        await result
            .Should()
            .ThrowAsync<ShortLinkNotFoundException>();
    }
    
    [Test]
    public async Task Handle_Should_ThrowBarcodeGenerationException()
    {
        // arange
        var shortLink = new ShortLink(_rnd.GetString(), 5)
        {
            Id = _rnd.Next()
        };
        
        var barcodeGenerationOptionsMock = new Mock<IOptions<BarcodeGenerationOptions>>();
        var barcodeStorageOptionsMock = new Mock<IOptions<BarcodeStorageOptions>>();
        var shortLinkRepositoryMock = new Mock<IShortLinkRepository>();
        var barcodeInfoRepositoryMock = new Mock<IBarcodeInfoRepository>();
        var barcodeGenerationServiceMock = new Mock<IBarcodeGenerationService>();
        var barcodeStorageServiceMock = new Mock<IBarcodeStorageService>();

        barcodeGenerationOptionsMock
            .Setup(c => c.Value)
            .Returns(new BarcodeGenerationOptions(_rnd.GetString()));
        
        barcodeStorageOptionsMock
            .Setup(c => c.Value)
            .Returns(new BarcodeStorageOptions(_rnd.GetString()));
        
        shortLinkRepositoryMock
            .Setup(c => c.GetShortLinkByToken(shortLink.Token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(shortLink);
        
        barcodeInfoRepositoryMock
            .Setup(c => c.GetBarcodeInfoByShortLinkId(shortLink.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((BarcodeInfo?)null);

        barcodeGenerationServiceMock
            .Setup(c => c.GenerateAsImageStream(string.Format(barcodeGenerationOptionsMock.Object.Value.BaseUrl, shortLink.Token)))
            .Throws(new Exception());
        
        var sut = new GetOrCreateBarcodeByShortLinkToken.Handler(
            _logger,
            shortLinkRepositoryMock.Object,
            barcodeInfoRepositoryMock.Object,
            barcodeGenerationServiceMock.Object,
            barcodeStorageServiceMock.Object,
            barcodeGenerationOptionsMock.Object,
            barcodeStorageOptionsMock.Object);

        // act
        Func<Task> result = () => sut.Handle(new GetOrCreateBarcodeByShortLinkToken.Command(shortLink.Token), CancellationToken.None);
        
        // assert
        await result
            .Should()
            .ThrowAsync<BarcodeGenerationException>();
    }
    
    [Test]
    public async Task Handle_Should_ThrowBarcodeSavingException()
    {
        // arange
        var bufferSize = 5;
        var buffer = Enumerable.Repeat(_rnd.NextByte(), bufferSize);
        
        await using var barcodeStream = new MemoryStream(buffer.ToArray());
        
        var shortLink = new ShortLink(_rnd.GetString(), 5)
        {
            Id = _rnd.Next()
        };
        
        var barcodeGenerationOptionsMock = new Mock<IOptions<BarcodeGenerationOptions>>();
        var barcodeStorageOptionsMock = new Mock<IOptions<BarcodeStorageOptions>>();
        var shortLinkRepositoryMock = new Mock<IShortLinkRepository>();
        var barcodeInfoRepositoryMock = new Mock<IBarcodeInfoRepository>();
        var barcodeGenerationServiceMock = new Mock<IBarcodeGenerationService>();
        var barcodeStorageServiceMock = new Mock<IBarcodeStorageService>();

        barcodeGenerationOptionsMock
            .Setup(c => c.Value)
            .Returns(new BarcodeGenerationOptions(_rnd.GetString()));
        
        barcodeStorageOptionsMock
            .Setup(c => c.Value)
            .Returns(new BarcodeStorageOptions(_rnd.GetString()));
        
        shortLinkRepositoryMock
            .Setup(c => c.GetShortLinkByToken(shortLink.Token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(shortLink);
        
        barcodeInfoRepositoryMock
            .Setup(c => c.GetBarcodeInfoByShortLinkId(shortLink.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((BarcodeInfo?)null);

        barcodeGenerationServiceMock
            .Setup(c => c.GenerateAsImageStream(string.Format(barcodeGenerationOptionsMock.Object.Value.BaseUrl, shortLink.Token)))
            .Returns(barcodeStream);

        barcodeStorageServiceMock
            .Setup(c => 
                c.SaveBarcode(
                    barcodeStream, 
                    Path.Combine(barcodeStorageOptionsMock.Object.Value.BasePath, shortLink.Token),
                It.IsAny<CancellationToken>()))
            .Throws(new Exception());
        
        var sut = new GetOrCreateBarcodeByShortLinkToken.Handler(
            _logger,
            shortLinkRepositoryMock.Object,
            barcodeInfoRepositoryMock.Object,
            barcodeGenerationServiceMock.Object,
            barcodeStorageServiceMock.Object,
            barcodeGenerationOptionsMock.Object,
            barcodeStorageOptionsMock.Object);

        // act
        Func<Task> result = () => sut.Handle(new GetOrCreateBarcodeByShortLinkToken.Command(shortLink.Token), CancellationToken.None);
        
        // assert
        await result
            .Should()
            .ThrowAsync<BarcodeSavingException>();
    }
    
    [Test]
    public async Task Handle_Should_ThrowBarcodeInfoSavingException()
    {
        // arange
        var bufferSize = 5;
        var buffer = Enumerable.Repeat(_rnd.NextByte(), bufferSize);
        
        await using var barcodeStream = new MemoryStream(buffer.ToArray());
        
        var shortLink = new ShortLink(_rnd.GetString(), 5)
        {
            Id = _rnd.Next()
        };
        
        var barcodeGenerationOptionsMock = new Mock<IOptions<BarcodeGenerationOptions>>();
        var barcodeStorageOptionsMock = new Mock<IOptions<BarcodeStorageOptions>>();
        var shortLinkRepositoryMock = new Mock<IShortLinkRepository>();
        var barcodeInfoRepositoryMock = new Mock<IBarcodeInfoRepository>();
        var barcodeGenerationServiceMock = new Mock<IBarcodeGenerationService>();
        var barcodeStorageServiceMock = new Mock<IBarcodeStorageService>();

        barcodeGenerationOptionsMock
            .Setup(c => c.Value)
            .Returns(new BarcodeGenerationOptions(_rnd.GetString()));
        
        barcodeStorageOptionsMock
            .Setup(c => c.Value)
            .Returns(new BarcodeStorageOptions(_rnd.GetString()));
        
        shortLinkRepositoryMock
            .Setup(c => c.GetShortLinkByToken(shortLink.Token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(shortLink);
        
        barcodeInfoRepositoryMock
            .Setup(c => c.GetBarcodeInfoByShortLinkId(shortLink.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((BarcodeInfo?)null);

        barcodeGenerationServiceMock
            .Setup(c => c.GenerateAsImageStream(string.Format(barcodeGenerationOptionsMock.Object.Value.BaseUrl, shortLink.Token)))
            .Returns(barcodeStream);

        barcodeInfoRepositoryMock
            .Setup(
                c =>
                    c.AddBarcodeInfo(
                        It.Is<BarcodeInfo>(
                            x =>
                                x.Path == Path.Combine(barcodeStorageOptionsMock.Object.Value.BasePath, shortLink.Token) &&
                                x.ShortLinkId == shortLink.Id),
                        It.IsAny<CancellationToken>()))
            .Throws(new Exception());
        
        var sut = new GetOrCreateBarcodeByShortLinkToken.Handler(
            _logger,
            shortLinkRepositoryMock.Object,
            barcodeInfoRepositoryMock.Object,
            barcodeGenerationServiceMock.Object,
            barcodeStorageServiceMock.Object,
            barcodeGenerationOptionsMock.Object,
            barcodeStorageOptionsMock.Object);

        // act
        Func<Task> result = () => sut.Handle(new GetOrCreateBarcodeByShortLinkToken.Command(shortLink.Token), CancellationToken.None);
        
        // assert
        await result
            .Should()
            .ThrowAsync<BarcodeInfoSavingException>();
    }
    
    [Test]
    public async Task Handle_Should_ThrowBarcodeReadingException()
    {
        // arange
        var bufferSize = 5;
        var buffer = Enumerable.Repeat(_rnd.NextByte(), bufferSize);
        
        await using var barcodeStream = new MemoryStream(buffer.ToArray());
        
        var shortLink = new ShortLink(_rnd.GetString(), 5)
        {
            Id = _rnd.Next()
        };
        
        var barcodeInfo = new BarcodeInfo(_rnd.GetString(), shortLink.Id);

        var barcodeGenerationOptionsMock = new Mock<IOptions<BarcodeGenerationOptions>>();
        var barcodeStorageOptionsMock = new Mock<IOptions<BarcodeStorageOptions>>();
        var shortLinkRepositoryMock = new Mock<IShortLinkRepository>();
        var barcodeInfoRepositoryMock = new Mock<IBarcodeInfoRepository>();
        var barcodeGenerationServiceMock = new Mock<IBarcodeGenerationService>();
        var barcodeStorageServiceMock = new Mock<IBarcodeStorageService>();

        barcodeGenerationOptionsMock
            .Setup(c => c.Value)
            .Returns(new BarcodeGenerationOptions(_rnd.GetString()));
        
        barcodeStorageOptionsMock
            .Setup(c => c.Value)
            .Returns(new BarcodeStorageOptions(_rnd.GetString()));
        
        shortLinkRepositoryMock
            .Setup(c => c.GetShortLinkByToken(shortLink.Token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(shortLink);
        
        barcodeInfoRepositoryMock
            .Setup(c => c.GetBarcodeInfoByShortLinkId(shortLink.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(barcodeInfo);

        barcodeStorageServiceMock
            .Setup(c => c.ReadBarcode(barcodeInfo.Path))
            .Throws(new Exception());
        
        var sut = new GetOrCreateBarcodeByShortLinkToken.Handler(
            _logger,
            shortLinkRepositoryMock.Object,
            barcodeInfoRepositoryMock.Object,
            barcodeGenerationServiceMock.Object,
            barcodeStorageServiceMock.Object,
            barcodeGenerationOptionsMock.Object,
            barcodeStorageOptionsMock.Object);

        // act
        Func<Task> result = () => sut.Handle(new GetOrCreateBarcodeByShortLinkToken.Command(shortLink.Token), CancellationToken.None);
        
        // assert
        await result
            .Should()
            .ThrowAsync<BarcodeReadingException>();
    }
}