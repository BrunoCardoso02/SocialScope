namespace SocialScope.Application.Features.Auth.Register;

public sealed record RegisterUserCommand(string Email, string Password, string Name);
