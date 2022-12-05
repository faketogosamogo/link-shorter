using FluentAssertions;
using LinkShorter.Application.Exceptions;
using LinkShorter.Application.Features.ShortLinks.Commands;
using LinkShorter.Application.Options;
using LinkShorter.Core.Entities;
using LinkShorter.Core.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace LinkShorter.Application.Tests;

[TestFixture]
public class CreateShortLinkTests
{
    private ILogger<CreateShortLink.Handler> _logger = new Mock<ILogger<CreateShortLink.Handler>>().Object;
        
    private Randomizer _rnd = new Randomizer();

    [Test]
    public async Task Handle_Should_CreateNewShortLink()
    {
        // arange
        var tokenLength = 5;
        var url = _rnd.GetString();
        
        var shortLinkRepositoryMock = new Mock<IShortLinkRepository>();
        var shortLinkOptionsMock = new Mock<IOptions<ShortLinkOptions>>();

        shortLinkOptionsMock
            .Setup(c => c.Value)
            .Returns(new ShortLinkOptions(tokenLength));
        
        shortLinkRepositoryMock
            .Setup(c => c.GetShortLinkByUrl(url, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ShortLink?)null);
        
        shortLinkRepositoryMock
            .Setup(c => c.IsShortLinkByTokenExist(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        
        var sut = new CreateShortLink.Handler(_logger, shortLinkRepositoryMock.Object, shortLinkOptionsMock.Object);
        
        // act
        var result = await sut.Handle(new CreateShortLink.Command(url), CancellationToken.None);

        // assert
        shortLinkRepositoryMock
            .Verify(
                c => 
                    c.AddShortLink(It.Is<ShortLink>(
                        c => c.Url == url && c.Token.Length == tokenLength), It.IsAny<CancellationToken>()),
                Times.Once);

        result.Token.Length
            .Should()
            .Be(tokenLength);
        result.Url
            .Should()
            .Be(url);
    }
    
    [Test]
    public async Task Handle_Should_ReturnExistShortLink()
    {
        // arange
        var existShortLink = new ShortLink(_rnd.GetString(), 5);
        existShortLink.Id = _rnd.Next();
        
        var shortLinkRepositoryMock = new Mock<IShortLinkRepository>();
        var shortLinkOptionsMock = new Mock<IOptions<ShortLinkOptions>>();

        shortLinkOptionsMock
            .Setup(c => c.Value)
            .Returns(new ShortLinkOptions(5));
        
        shortLinkRepositoryMock
            .Setup(c => c.GetShortLinkByUrl(existShortLink.Url, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existShortLink);

        var sut = new CreateShortLink.Handler(_logger, shortLinkRepositoryMock.Object, shortLinkOptionsMock.Object);

        // act
        var result = await sut.Handle(new CreateShortLink.Command(existShortLink.Url), CancellationToken.None);

        // assert
        result.Id
            .Should()
            .Be(existShortLink.Id);
        
        result.Token
            .Should()
            .Be(existShortLink.Token);
        
        result.Url
            .Should()
            .Be(existShortLink.Url);
    }
    
    [Test]
    public async Task Handle_Should_ThrowInvalidTokenGenerationException()
    {
        // arange
        var tokenLength = 5;
        var url = _rnd.GetString();
        
        var shortLinkRepositoryMock = new Mock<IShortLinkRepository>();
        var shortLinkOptionsMock = new Mock<IOptions<ShortLinkOptions>>();

        shortLinkOptionsMock
            .Setup(c => c.Value)
            .Returns(new ShortLinkOptions(tokenLength));

        shortLinkRepositoryMock
            .Setup(c => c.GetShortLinkByUrl(url, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ShortLink?)null);
        
        shortLinkRepositoryMock
            .Setup(c => c.IsShortLinkByTokenExist(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        
        var sut = new CreateShortLink.Handler(_logger, shortLinkRepositoryMock.Object, shortLinkOptionsMock.Object);

        
        // act
        Func<Task> result = () => sut.Handle(new CreateShortLink.Command(url), CancellationToken.None);

        // assert
        await result
            .Should()
            .ThrowAsync<InvalidTokenGenerationException>();
    }
    
    [Test]
    public async Task Handle_Should_ThrowShortLinkSavingException()
    {
        // arange
        var tokenLength = 5;
        var url = _rnd.GetString();
        
        var shortLinkRepositoryMock = new Mock<IShortLinkRepository>();
        var shortLinkOptionsMock = new Mock<IOptions<ShortLinkOptions>>();

        shortLinkRepositoryMock
            .Setup(c => c.GetShortLinkByUrl(url, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ShortLink?)null);
        
        shortLinkRepositoryMock
            .Setup(c => c.IsShortLinkByTokenExist(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        
        shortLinkRepositoryMock
            .Setup(c => c.AddShortLink(It.IsAny<ShortLink>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception());
        
        shortLinkOptionsMock
            .Setup(c => c.Value)
            .Returns(new ShortLinkOptions(tokenLength));
        
        var sut = new CreateShortLink.Handler(_logger, shortLinkRepositoryMock.Object, shortLinkOptionsMock.Object);

        
        // act
        Func<Task> result = () => sut.Handle(new CreateShortLink.Command(url), CancellationToken.None);

        // assert
        await result
            .Should()
            .ThrowAsync<ShortLinkSavingException>();
    }
}