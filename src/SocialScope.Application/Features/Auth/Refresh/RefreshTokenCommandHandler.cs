using SocialScope.Application.Exceptions;
using SocialScope.Application.Interfaces;
using SocialScope.Domain.Entities;

namespace SocialScope.Application.Features.Auth.Refresh;

public sealed class RefreshTokenCommandHandler
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IRefreshTokenGenerator _refreshTokenGenerator;
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public RefreshTokenCommandHandler(
        IRefreshTokenRepository refreshTokenRepository,
        IRefreshTokenGenerator refreshTokenGenerator,
        IUserRepository userRepository,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _refreshTokenGenerator = refreshTokenGenerator;
        _userRepository = userRepository;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<RefreshTokenResponse> Handle(RefreshTokenCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.RefreshToken))
        {
            throw new InvalidRefreshTokenException();
        }

        var tokenHash = _refreshTokenGenerator.Hash(command.RefreshToken);

        var storedToken = await _refreshTokenRepository.GetRefreshTokenByHashAsync(tokenHash, cancellationToken)
            ?? throw new InvalidRefreshTokenException();

        if (storedToken.IsRevoked)
        {
            await _refreshTokenRepository.RevokeAllUserRefreshTokensAsync(storedToken.UserId, cancellationToken);
            throw new InvalidRefreshTokenException();
        }

        if (storedToken.IsExpired(DateTimeOffset.UtcNow))
        {
            throw new InvalidRefreshTokenException();
        }

        var user = await _userRepository.GetUserByIdAsync(storedToken.UserId, cancellationToken)
            ?? throw new InvalidRefreshTokenException();

        storedToken.Revoke();
        await _refreshTokenRepository.UpdateRefreshTokenAsync(storedToken, cancellationToken);

        var newRefreshToken = _refreshTokenGenerator.Generate();
        await _refreshTokenRepository.AddRefreshTokenAsync(
            RefreshToken.Create(user.Id, newRefreshToken.TokenHash, newRefreshToken.ExpiresAt),
            cancellationToken);

        var (accessToken, accessTokenExpiresAt) = _jwtTokenGenerator.Generate(user);

        return new RefreshTokenResponse(
            accessToken,
            accessTokenExpiresAt,
            newRefreshToken.Token,
            newRefreshToken.ExpiresAt);
    }
}
