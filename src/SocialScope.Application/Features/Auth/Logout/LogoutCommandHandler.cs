using SocialScope.Application.Interfaces;

namespace SocialScope.Application.Features.Auth.Logout;

public sealed class LogoutCommandHandler
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IRefreshTokenGenerator _refreshTokenGenerator;

    public LogoutCommandHandler(
        IRefreshTokenRepository refreshTokenRepository,
        IRefreshTokenGenerator refreshTokenGenerator)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _refreshTokenGenerator = refreshTokenGenerator;
    }

    public async Task Handle(LogoutCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.RefreshToken))
        {
            return;
        }

        var tokenHash = _refreshTokenGenerator.Hash(command.RefreshToken);

        var storedToken = await _refreshTokenRepository.GetRefreshTokenByHashAsync(tokenHash, cancellationToken);
        if (storedToken is null || storedToken.IsRevoked)
        {
            return;
        }

        storedToken.Revoke();
        await _refreshTokenRepository.UpdateRefreshTokenAsync(storedToken, cancellationToken);
    }
}
