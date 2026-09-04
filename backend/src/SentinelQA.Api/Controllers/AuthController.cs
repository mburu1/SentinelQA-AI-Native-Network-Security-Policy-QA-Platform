using MediatR;
using Microsoft.AspNetCore.Mvc;
using SentinelQA.Modules.Identity.Features.Login;
using SentinelQA.Modules.Identity.Features.RefreshToken;
using SentinelQA.Modules.Identity.Features.Register;

namespace SentinelQA.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public sealed class AuthController(IMediator mediator) : ControllerBase
{
    [HttpPost("register")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<IActionResult> Register([FromBody] RegisterUserCommand command, CancellationToken cancellationToken)
    {
        var userId = await mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(Register), userId);
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> Login([FromBody] LoginCommand command, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(command, cancellationToken));

    [HttpPost("refresh")]
    [ProducesResponseType(typeof(RefreshTokenResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenCommand command, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(command, cancellationToken));
}