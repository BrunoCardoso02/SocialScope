using SocialScope.Domain.Entities;

namespace SocialScope.UnitTests.Entities;

public class RefreshTokenTests
{
    [Fact]
    public void IsActive_DeveSerVerdadeiro_QuandoNaoRevogadoENaoExpirado()
    {
        var token = RefreshToken.Create(Guid.NewGuid(), "hash", DateTimeOffset.UtcNow.AddDays(7));

        Assert.True(token.IsActive(DateTimeOffset.UtcNow));
    }

    [Fact]
    public void IsActive_DeveSerFalso_QuandoExpirado()
    {
        var token = RefreshToken.Create(Guid.NewGuid(), "hash", DateTimeOffset.UtcNow.AddMinutes(-1));

        Assert.True(token.IsExpired(DateTimeOffset.UtcNow));
        Assert.False(token.IsActive(DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Revoke_DeveMarcarComoRevogadoEManterDataOriginalEmChamadasRepetidas()
    {
        var token = RefreshToken.Create(Guid.NewGuid(), "hash", DateTimeOffset.UtcNow.AddDays(7));

        token.Revoke();
        var firstRevokedAt = token.RevokedAt;
        token.Revoke();

        Assert.True(token.IsRevoked);
        Assert.False(token.IsActive(DateTimeOffset.UtcNow));
        Assert.Equal(firstRevokedAt, token.RevokedAt);
    }
}
