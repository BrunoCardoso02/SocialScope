namespace SocialScope.Domain.Entities;

public sealed class RefreshToken
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string TokenHash { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset ExpiresAt { get; private set; }
    public DateTimeOffset? RevokedAt { get; private set; }

    private RefreshToken(
        Guid id,
        Guid userId,
        string tokenHash,
        DateTimeOffset createdAt,
        DateTimeOffset expiresAt,
        DateTimeOffset? revokedAt)
    {
        Id = id;
        UserId = userId;
        TokenHash = tokenHash;
        CreatedAt = createdAt;
        ExpiresAt = expiresAt;
        RevokedAt = revokedAt;
    }

    public static RefreshToken Create(Guid userId, string tokenHash, DateTimeOffset expiresAt)
    {
        return new RefreshToken(
            Guid.NewGuid(),
            userId,
            tokenHash,
            DateTimeOffset.UtcNow,
            expiresAt,
            null);
    }

    public bool IsRevoked => RevokedAt.HasValue;

    public bool IsExpired(DateTimeOffset now) => now >= ExpiresAt;

    public bool IsActive(DateTimeOffset now) => !IsRevoked && !IsExpired(now);

    public void Revoke()
    {
        RevokedAt ??= DateTimeOffset.UtcNow;
    }
}
