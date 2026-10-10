using SocialScope.Application.Exceptions;
using SocialScope.Application.Interfaces;
using SocialScope.Domain.Entities;
using SocialScope.Domain.ValueObjects;

namespace SocialScope.Application.Features.Auth.Login;

public sealed class LoginCommandHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IRefreshTokenGenerator _refreshTokenGenerator;
    private readonly IRefreshTokenRepository _refreshTokenRepository;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        IRefreshTokenGenerator refreshTokenGenerator,
        IRefreshTokenRepository refreshTokenRepository)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _refreshTokenGenerator = refreshTokenGenerator;
        _refreshTokenRepository = refreshTokenRepository;
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

        var (accessToken, accessTokenExpiresAt) = _jwtTokenGenerator.Generate(user);

        var refreshToken = _refreshTokenGenerator.Generate();
        await _refreshTokenRepository.AddRefreshTokenAsync(
            RefreshToken.Create(user.Id, refreshToken.TokenHash, refreshToken.ExpiresAt),
            cancellationToken);

        return new LoginResponse(
            accessToken,
            accessTokenExpiresAt,
            refreshToken.Token,
            refreshToken.ExpiresAt);
    }
}
