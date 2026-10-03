using SocialScope.Domain.ValueObjects;

namespace SocialScope.Domain.Entities;

public sealed class User
{
    public Guid Id { get; private set; }
    public Email Email { get; private set; }
    public string PasswordHash { get; private set; }
    public string Name { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public DateTimeOffset? LastLoginAt { get; private set; }

    private User
        (
            Guid id,
            Email email,
            string passwordHash,
            string name,
            DateTimeOffset createdAt,
            DateTimeOffset updatedAt,
            DateTimeOffset? lastLoginAt
        )
    {
        Id = id;
        Email = email;
        PasswordHash = passwordHash;
        Name = name;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
        LastLoginAt = lastLoginAt;
    }

    public static User Create(string email, string passwordHash, string name)
    {
        return new User(
            Guid.NewGuid(),
            Email.Create(email),
            passwordHash,
            name,
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow,
            null);
    }

    public void RegisterLogin()
    {
        LastLoginAt = DateTimeOffset.UtcNow;
    }
}
