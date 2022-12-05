using FluentAssertions;
using LinkShorter.Application.Exceptions;
using LinkShorter.Application.Features.ShortLinks.Queries;
using LinkShorter.Core.Entities;
using LinkShorter.Core.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace LinkShorter.Application.Tests;

[TestFixture]
public class GetShortLinkByTokenTests
{
    private ILogger<GetShortLinkByToken.Handler> _logger = new Mock<ILogger<GetShortLinkByToken.Handler>>().Object;
        
    private Randomizer _rnd = new Randomizer();

    [Test]
    public async Task Handle_Should_ReturnExistShortLink()
    {
        // arange
        var url = "url";
        var shortLink = new ShortLink(url, 5);
        
        var shortLinkRepositoryMock = new Mock<IShortLinkRepository>();

        shortLinkRepositoryMock
            .Setup(c => c.GetShortLinkByToken(shortLink.Token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(shortLink);

        var sut = new GetShortLinkByToken.Handler(_logger, shortLinkRepositoryMock.Object);
        
        // act
        var result = await sut.Handle(new GetShortLinkByToken.Query(shortLink.Token), CancellationToken.None);

        // assert
        result.Token
            .Should()
            .Be(shortLink.Token);

        result.Url
            .Should()
            .Be(shortLink.Url);
    }
    
    [Test]
    public async Task Handle_Should_ThrowShortLinkNotFoundException()
    {
        // arange
        var shortLinkRepositoryMock = new Mock<IShortLinkRepository>();

        shortLinkRepositoryMock
            .Setup(c => c.GetShortLinkByToken(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ShortLink?)null);

        var sut = new GetShortLinkByToken.Handler(_logger, shortLinkRepositoryMock.Object);
        
        // act
        Func<Task> result = () => sut.Handle(new GetShortLinkByToken.Query(_rnd.GetString()), CancellationToken.None);

        // assert
        await result.Should()
            .ThrowAsync<ShortLinkNotFoundException>();
    }
}