using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SentinelQA.Modules.Policies.Features.CreatePolicy;
using SentinelQA.Modules.Policies.Features.GetPolicy;
using SentinelQA.Modules.Policies.Features.ListPolicies;
using SentinelQA.Modules.Policies.Features.ValidatePolicy;

namespace SentinelQA.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/policies")]
public sealed class PoliciesController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = "Admin,SecurityEngineer")]
    [ProducesResponseType(typeof(CreatePolicyResult), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreatePolicyCommand command, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);
        return Created($"/api/v1/policies/{result.PolicyId}", result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PolicyDetails), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var policy = await mediator.Send(new GetPolicyQuery(id), cancellationToken);
        return policy is null ? NotFound() : Ok(policy);
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] Guid? firewallId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default) =>
        Ok(await mediator.Send(new ListPoliciesQuery(firewallId, page, pageSize), cancellationToken));

    [HttpPost("{id:guid}/validate")]
    [Authorize(Roles = "Admin,SecurityEngineer,QAEngineer")]
    [ProducesResponseType(typeof(ValidatePolicyResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> Validate(Guid id, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new ValidatePolicyCommand(id), cancellationToken));
}