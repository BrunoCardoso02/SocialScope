using SocialScope.Domain.Entities;

namespace SocialScope.Application.Interfaces;

public interface IJwtTokenGenerator
{
    (string Token, DateTimeOffset ExpiresAt) Generate(User user);
}
