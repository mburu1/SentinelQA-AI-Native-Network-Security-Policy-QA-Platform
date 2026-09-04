using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SentinelQA.Modules.Testing.Features.GetTestRun;
using SentinelQA.Modules.Testing.Features.StartTestRun;

namespace SentinelQA.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/test-runs")]
public sealed class TestRunsController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = "Admin,QAEngineer,SecurityEngineer")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status202Accepted)]
    public async Task<IActionResult> Start([FromBody] StartTestRunCommand command, CancellationToken cancellationToken)
    {
        var testRunId = await mediator.Send(command, cancellationToken);
        return Accepted($"/api/v1/test-runs/{testRunId}", testRunId);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TestRunDetails), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var run = await mediator.Send(new GetTestRunQuery(id), cancellationToken);
        return run is null ? NotFound() : Ok(run);
    }
}