using LinkShorter.Api.Models.ShortLinks;
using LinkShorter.Application.Features.ShortLinks.Commands;
using LinkShorter.Application.Features.ShortLinks.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LinkShorter.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class ShortLinksController : ControllerBase
{
    private readonly IMediator _mediator;

    public ShortLinksController(
        IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<CreateShortLinkResponse> CreateShortLink(CreateShortLinkRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateShortLink.Command(request.Url);

        var response = await _mediator.Send(command, cancellationToken);

        return new CreateShortLinkResponse(response.Token);
    }

    [HttpGet("token/{token}")]
    public async Task<IActionResult> GetShortLinkByTokenWithRedirect(string token, CancellationToken cancellationToken)
    {
        var command = new GetShortLinkByToken.Query(token);

        var shortLink = await _mediator.Send(command, cancellationToken);

        return Redirect(shortLink.Url);
    }
}