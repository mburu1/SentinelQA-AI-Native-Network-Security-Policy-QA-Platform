using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SentinelQA.Modules.Defects.Features.CreateDefect;
using SentinelQA.Modules.Defects.Features.TransitionDefect;

namespace SentinelQA.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/defects")]
public sealed class DefectsController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = "Admin,QAEngineer,SecurityEngineer,Developer")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateDefectCommand command, CancellationToken cancellationToken)
    {
        var defectId = await mediator.Send(command, cancellationToken);
        return Created($"/api/v1/defects/{defectId}", defectId);
    }

    [HttpPost("{id:guid}/transition")]
    [Authorize(Roles = "Admin,QAEngineer,Developer")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Transition(Guid id, [FromBody] TransitionDefectCommand command, CancellationToken cancellationToken)
    {
        await mediator.Send(command with { DefectId = id }, cancellationToken);
        return Ok();
    }
}