using SocialScope.Application.Exceptions;
using SocialScope.Application.Interfaces;
using SocialScope.Domain.Entities;
using SocialScope.Domain.ValueObjects;

namespace SocialScope.Application.Features.Auth.Register;

public sealed class RegisterUserCommandHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public RegisterUserCommandHandler(IUserRepository userRepository, IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<RegisterUserResponse> Handle(RegisterUserCommand command, CancellationToken cancellationToken)
    {
        var email = Email.Create(command.Email);

        var existingUser = await _userRepository.GetUserByEmailAsync(email, cancellationToken);
        if (existingUser is not null)
        {
            throw new EmailAlreadyInUseException(command.Email);
        }

        var passwordHash = _passwordHasher.Hash(command.Password);

        var user = User.Create(command.Email, passwordHash, command.Name);

        await _userRepository.AddUserAsync(user, cancellationToken);

        return new RegisterUserResponse(user.Id, user.Email.Value, user.Name, user.CreatedAt);
    }
}
