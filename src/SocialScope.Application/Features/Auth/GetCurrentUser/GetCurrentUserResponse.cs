namespace SocialScope.Application.Features.Auth.GetCurrentUser;

public sealed record GetCurrentUserResponse(
    Guid Id,
    string Email,
    string Name,
    DateTimeOffset CreatedAt,
    DateTimeOffset? LastLoginAt);
