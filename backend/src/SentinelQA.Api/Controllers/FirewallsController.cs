using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SentinelQA.Modules.Firewalls.Features.RegisterFirewall;

namespace SentinelQA.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/firewalls")]
public sealed class FirewallsController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = "Admin,SecurityEngineer")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<IActionResult> Register([FromBody] RegisterFirewallCommand command, CancellationToken cancellationToken)
    {
        var firewallId = await mediator.Send(command, cancellationToken);
        return Created($"/api/v1/firewalls/{firewallId}", firewallId);
    }
}