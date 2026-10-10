using Microsoft.Extensions.Options;
using SocialScope.Infrastructure.Identity;

namespace SocialScope.UnitTests.Identity;

public class RefreshTokenGeneratorTests
{
    private static RefreshTokenGenerator CreateGenerator(int expirationDays = 7)
    {
        return new RefreshTokenGenerator(Options.Create(new JwtSettings { RefreshTokenExpirationDays = expirationDays }));
    }

    [Fact]
    public void Generate_DeveGerarTokensDiferentesACadaChamada()
    {
        var generator = CreateGenerator();

        var first = generator.Generate();
        var second = generator.Generate();

        Assert.NotEqual(first.Token, second.Token);
        Assert.NotEqual(first.TokenHash, second.TokenHash);
    }

    [Fact]
    public void Generate_DeveRetornarHashDoToken_ENaoOTokenEmSi()
    {
        var generator = CreateGenerator();

        var generated = generator.Generate();

        Assert.NotEqual(generated.Token, generated.TokenHash);
        Assert.Equal(generator.Hash(generated.Token), generated.TokenHash);
    }

    [Fact]
    public void Generate_DeveUsarAExpiracaoConfigurada()
    {
        var generator = CreateGenerator(expirationDays: 3);

        var generated = generator.Generate();

        var expected = DateTimeOffset.UtcNow.AddDays(3);
        Assert.InRange(generated.ExpiresAt, expected.AddSeconds(-5), expected.AddSeconds(5));
    }
}
