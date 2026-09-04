using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SentinelQA.Api.Authorization;
using SentinelQA.Api.Filters;
using SentinelQA.Modules.ChangeManagement.Features.ApproveChangeRequest;
using SentinelQA.Modules.ChangeManagement.Features.DeployChangeRequest;
using SentinelQA.Modules.ChangeManagement.Features.GetChangeRequest;
using SentinelQA.Modules.ChangeManagement.Features.SubmitChangeRequest;

namespace SentinelQA.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/change-requests")]
[ServiceFilter(typeof(IdempotencyFilter))]
public sealed class ChangeRequestsController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = "Admin,SecurityEngineer,Developer")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status202Accepted)]
    public async Task<IActionResult> Submit([FromBody] SubmitChangeRequestCommand command, CancellationToken cancellationToken)
    {
        var changeRequestId = await mediator.Send(command, cancellationToken);
        return Accepted($"/api/v1/change-requests/{changeRequestId}", changeRequestId);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ChangeRequestDetails), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var changeRequest = await mediator.Send(new GetChangeRequestQuery(id), cancellationToken);
        return changeRequest is null ? NotFound() : Ok(changeRequest);
    }

    [HttpPost("{id:guid}/approve")]
    [Authorize(Policy = PolicyNames.CanApproveChange)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Approve(Guid id, CancellationToken cancellationToken)
    {
        await mediator.Send(new ApproveChangeRequestCommand(id), cancellationToken);
        return Ok();
    }

    [HttpPost("{id:guid}/deploy")]
    [Authorize(Policy = PolicyNames.CanDeployPolicy)]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> Deploy(Guid id, CancellationToken cancellationToken)
    {
        await mediator.Send(new DeployChangeRequestCommand(id), cancellationToken);
        return Accepted();
    }
}