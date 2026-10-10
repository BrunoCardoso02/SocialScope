using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SocialScope.Application.Interfaces;

namespace SocialScope.Infrastructure.Identity;

public sealed class RefreshTokenGenerator : IRefreshTokenGenerator
{
    private const int TokenSizeInBytes = 64;

    private readonly JwtSettings _settings;

    public RefreshTokenGenerator(IOptions<JwtSettings> settings)
    {
        _settings = settings.Value;
    }

    public GeneratedRefreshToken Generate()
    {
        var token = Base64UrlEncoder.Encode(RandomNumberGenerator.GetBytes(TokenSizeInBytes));
        var expiresAt = DateTimeOffset.UtcNow.AddDays(_settings.RefreshTokenExpirationDays);

        return new GeneratedRefreshToken(token, Hash(token), expiresAt);
    }

    public string Hash(string token)
    {
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
    }
}
