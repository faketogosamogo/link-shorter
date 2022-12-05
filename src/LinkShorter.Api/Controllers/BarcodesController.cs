using LinkShorter.Application.Features.Barcode.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LinkShorter.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class BarcodesController : ControllerBase
{
    private readonly IMediator _mediator;

    public BarcodesController(
        IMediator mediator)
    {
        _mediator = mediator;
    }
    
    [HttpGet("token/{token}")]
    public async Task<FileStreamResult> GetOrCreateBarcodeByShortLinkToken(string token, CancellationToken cancellationToken)
    {
        var command = new GetOrCreateBarcodeByShortLinkToken.Command(token);

        var shortLink = await _mediator.Send(command, cancellationToken);

        return new FileStreamResult(shortLink, "application/octet-stream");
    }
}