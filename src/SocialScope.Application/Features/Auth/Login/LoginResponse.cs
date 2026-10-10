namespace SocialScope.Application.Features.Auth.Login;

public sealed record LoginResponse(
    string AccessToken,
    DateTimeOffset ExpiresAt,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAt);
