using Moq;
using SocialScope.Application.Exceptions;
using SocialScope.Application.Features.Auth.Register;
using SocialScope.Application.Interfaces;
using SocialScope.Domain.Entities;
using SocialScope.Domain.ValueObjects;
using SocialScope.Infrastructure.Identity;

namespace SocialScope.UnitTests.Features.Auth.Register;

public class RegisterUserCommandHandlerTests
{
    [Fact]
    public async Task Handle_DeveCriarUsuario_QuandoEmailNaoExiste()
    {
        var userRepositoryMock = new Mock<IUserRepository>();
        var passwordHasherMock = new Mock<IPasswordHasher>();

        userRepositoryMock
            .Setup(r => r.GetUserByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        passwordHasherMock
            .Setup(h => h.Hash(It.IsAny<string>()))
            .Returns("hash-fake");

        var handler = new RegisterUserCommandHandler(userRepositoryMock.Object, passwordHasherMock.Object);
        var command = new RegisterUserCommand("ana@email.com", "senha123", "Ana");

        var response = await handler.Handle(command, CancellationToken.None);

        Assert.Equal("ana@email.com", response.Email);
        Assert.Equal("Ana", response.Name);
        userRepositoryMock.Verify(r => r.AddUserAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
        passwordHasherMock.Verify(h => h.Hash("senha123"), Times.Once);
    }

    [Fact]
    public async Task Handle_DeveLancarExcecao_QuandoEmailJaExiste()
    {
        var userExistente = User.Create("ana@email.com", "hash-antigo", "Ana");

        var userRepositoryMock = new Mock<IUserRepository>();
        userRepositoryMock
            .Setup(r => r.GetUserByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(userExistente);

        var handler = new RegisterUserCommandHandler(userRepositoryMock.Object, new PasswordHasher());
        var command = new RegisterUserCommand("ana@email.com", "senha123", "Ana");

        await Assert.ThrowsAsync<EmailAlreadyInUseException>(
            () => handler.Handle(command, CancellationToken.None));

        userRepositoryMock.Verify(r => r.AddUserAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
