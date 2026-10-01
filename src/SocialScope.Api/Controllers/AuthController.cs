using Microsoft.AspNetCore.Mvc;
using SocialScope.Application.Exceptions;
using SocialScope.Application.Features.Auth.Register;
using SocialScope.Domain.Exceptions;

namespace SocialScope.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly RegisterUserCommandHandler _registerUserCommandHandler;

    public AuthController(RegisterUserCommandHandler registerUserCommandHandler)
    {
        _registerUserCommandHandler = registerUserCommandHandler;
    }

    [HttpPost("register")]
    public async Task<ActionResult<RegisterUserResponse>> Register(
        [FromBody] RegisterUserCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await _registerUserCommandHandler.Handle(command, cancellationToken);
            return StatusCode(StatusCodes.Status201Created, response);
        }
        catch (EmailAlreadyInUseException ex)
        {
            return Conflict(new ProblemDetails
            {
                Title = "E-mail já cadastrado",
                Detail = ex.Message,
                Status = StatusCodes.Status409Conflict
            });
        }
        catch (InvalidEmailException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "E-mail inválido",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
    }
}
