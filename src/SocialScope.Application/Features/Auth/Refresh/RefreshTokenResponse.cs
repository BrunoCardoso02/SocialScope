namespace SocialScope.Application.Features.Auth.Refresh;

public sealed record RefreshTokenResponse(
    string AccessToken,
    DateTimeOffset ExpiresAt,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAt);
