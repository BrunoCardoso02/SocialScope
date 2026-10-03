using SocialScope.Application.Exceptions;
using SocialScope.Application.Interfaces;
using SocialScope.Domain.ValueObjects;

namespace SocialScope.Application.Features.Auth.Login;

public sealed class LoginCommandHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<LoginResponse> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        var email = Email.Create(command.Email);

        var user = await _userRepository.GetUserByEmailAsync(email, cancellationToken);
        if (user is null || !_passwordHasher.Verify(command.Password, user.PasswordHash))
        {
            throw new InvalidCredentialsException();
        }

        user.RegisterLogin();
        await _userRepository.UpdateUserAsync(user, cancellationToken);

        var (token, expiresAt) = _jwtTokenGenerator.Generate(user);

        return new LoginResponse(token, expiresAt);
    }
}
