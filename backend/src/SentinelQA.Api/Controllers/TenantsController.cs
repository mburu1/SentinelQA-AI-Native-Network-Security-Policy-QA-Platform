using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SentinelQA.Api.Authorization;
using SentinelQA.Modules.Tenants.Features.CreateTenant;

namespace SentinelQA.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/tenants")]
public sealed class TenantsController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    [Authorize(Policy = PolicyNames.CanManageUsers)]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateTenantCommand command, CancellationToken cancellationToken)
    {
        var tenantId = await mediator.Send(command, cancellationToken);
        return Created($"/api/v1/tenants/{tenantId}", tenantId);
    }
}