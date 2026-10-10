using Microsoft.EntityFrameworkCore;
using SocialScope.Application.Interfaces;
using SocialScope.Domain.Entities;

namespace SocialScope.Infrastructure.Persistence.Repositories;

public sealed class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly AppDbContext _dbContext;

    public RefreshTokenRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddRefreshTokenAsync(RefreshToken refreshToken, CancellationToken cancellationToken)
    {
        await _dbContext.RefreshTokens.AddAsync(refreshToken, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<RefreshToken?> GetRefreshTokenByHashAsync(string tokenHash, CancellationToken cancellationToken)
    {
        return _dbContext.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == tokenHash, cancellationToken);
    }

    public async Task UpdateRefreshTokenAsync(RefreshToken refreshToken, CancellationToken cancellationToken)
    {
        _dbContext.RefreshTokens.Update(refreshToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RevokeAllUserRefreshTokensAsync(Guid userId, CancellationToken cancellationToken)
    {
        var activeTokens = await _dbContext.RefreshTokens
            .Where(t => t.UserId == userId && t.RevokedAt == null)
            .ToListAsync(cancellationToken);

        foreach (var token in activeTokens)
        {
            token.Revoke();
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
