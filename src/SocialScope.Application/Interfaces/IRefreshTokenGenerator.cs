namespace SocialScope.Application.Interfaces;

public sealed record GeneratedRefreshToken(string Token, string TokenHash, DateTimeOffset ExpiresAt);

public interface IRefreshTokenGenerator
{
    GeneratedRefreshToken Generate();

    string Hash(string token);
}
