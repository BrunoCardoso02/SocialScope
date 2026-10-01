using SocialScope.Domain.Entities;
using SocialScope.Domain.ValueObjects;

namespace SocialScope.Application.Interfaces;

public interface IUserRepository
{
    Task AddUserAsync(User user, CancellationToken cancellationToken);

    Task<User?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<User?> GetUserByEmailAsync(Email email, CancellationToken cancellationToken);

    Task UpdateUserAsync(User user, CancellationToken cancellationToken);

    Task DeleteUserAsync(User user, CancellationToken cancellationToken);
}
