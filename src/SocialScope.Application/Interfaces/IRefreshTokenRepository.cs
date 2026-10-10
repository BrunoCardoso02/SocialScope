using SocialScope.Domain.Entities;

namespace SocialScope.Application.Interfaces;

public interface IRefreshTokenRepository
{
    Task AddRefreshTokenAsync(RefreshToken refreshToken, CancellationToken cancellationToken);

    Task<RefreshToken?> GetRefreshTokenByHashAsync(string tokenHash, CancellationToken cancellationToken);

    Task UpdateRefreshTokenAsync(RefreshToken refreshToken, CancellationToken cancellationToken);

    Task RevokeAllUserRefreshTokensAsync(Guid userId, CancellationToken cancellationToken);
}
