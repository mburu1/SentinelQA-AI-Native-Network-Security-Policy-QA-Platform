using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SentinelQA.Modules.Networks.Features.DefineNetwork;

namespace SentinelQA.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/networks")]
public sealed class NetworksController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = "Admin,SecurityEngineer")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<IActionResult> Define([FromBody] DefineNetworkCommand command, CancellationToken cancellationToken)
    {
        var networkId = await mediator.Send(command, cancellationToken);
        return Created($"/api/v1/networks/{networkId}", networkId);
    }
}