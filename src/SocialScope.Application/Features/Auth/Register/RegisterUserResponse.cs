namespace SocialScope.Application.Features.Auth.Register;

public sealed record RegisterUserResponse(
    Guid Id,
    string Email,
    string Name,
    DateTimeOffset CreatedAt);
