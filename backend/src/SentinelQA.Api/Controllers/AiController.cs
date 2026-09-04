using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SentinelQA.Modules.Ai.Features.GenerateTestScenarios;

namespace SentinelQA.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/ai")]
public sealed class AiController(IMediator mediator) : ControllerBase
{
    [HttpPost("test-scenarios")]
    [Authorize(Roles = "Admin,QAEngineer,SecurityEngineer")]
    [ProducesResponseType(typeof(GenerateTestScenariosResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> GenerateTestScenarios([FromBody] GenerateTestScenariosCommand command, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(command, cancellationToken));
}