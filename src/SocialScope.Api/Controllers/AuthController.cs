using Microsoft.AspNetCore.Mvc;
using SocialScope.Application.Exceptions;
using SocialScope.Application.Features.Auth.Login;
using SocialScope.Application.Features.Auth.Register;
using SocialScope.Domain.Exceptions;

namespace SocialScope.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly RegisterUserCommandHandler _registerUserCommandHandler;
    private readonly LoginCommandHandler _loginCommandHandler;

    public AuthController(
        RegisterUserCommandHandler registerUserCommandHandler,
        LoginCommandHandler loginCommandHandler)
    {
        _registerUserCommandHandler = registerUserCommandHandler;
        _loginCommandHandler = loginCommandHandler;
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

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(
        [FromBody] LoginCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await _loginCommandHandler.Handle(command, cancellationToken);
            return Ok(response);
        }
        catch (InvalidCredentialsException ex)
        {
            return Unauthorized(new ProblemDetails
            {
                Title = "Credenciais inválidas",
                Detail = ex.Message,
                Status = StatusCodes.Status401Unauthorized
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
