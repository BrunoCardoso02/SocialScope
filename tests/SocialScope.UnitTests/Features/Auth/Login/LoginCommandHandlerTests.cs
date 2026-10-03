using Moq;
using SocialScope.Application.Exceptions;
using SocialScope.Application.Features.Auth.Login;
using SocialScope.Application.Interfaces;
using SocialScope.Domain.Entities;
using SocialScope.Domain.ValueObjects;

namespace SocialScope.UnitTests.Features.Auth.Login;

public class LoginCommandHandlerTests
{
    [Fact]
    public async Task Handle_DeveRetornarToken_QuandoCredenciaisValidas()
    {
        var user = User.Create("ana@email.com", "hash-correto", "Ana");

        var userRepositoryMock = new Mock<IUserRepository>();
        userRepositoryMock
            .Setup(r => r.GetUserByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var passwordHasherMock = new Mock<IPasswordHasher>();
        passwordHasherMock
            .Setup(h => h.Verify("senha123", "hash-correto"))
            .Returns(true);

        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(15);
        var jwtTokenGeneratorMock = new Mock<IJwtTokenGenerator>();
        jwtTokenGeneratorMock
            .Setup(g => g.Generate(user))
            .Returns(("token-fake", expiresAt));

        var handler = new LoginCommandHandler(
            userRepositoryMock.Object,
            passwordHasherMock.Object,
            jwtTokenGeneratorMock.Object);

        var response = await handler.Handle(
            new LoginCommand("ana@email.com", "senha123"),
            CancellationToken.None);

        Assert.Equal("token-fake", response.AccessToken);
        Assert.Equal(expiresAt, response.ExpiresAt);
        userRepositoryMock.Verify(r => r.UpdateUserAsync(user, It.IsAny<CancellationToken>()), Times.Once);
        Assert.NotNull(user.LastLoginAt);
    }

    [Fact]
    public async Task Handle_DeveLancarExcecao_QuandoUsuarioNaoExiste()
    {
        var userRepositoryMock = new Mock<IUserRepository>();
        userRepositoryMock
            .Setup(r => r.GetUserByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var handler = new LoginCommandHandler(
            userRepositoryMock.Object,
            Mock.Of<IPasswordHasher>(),
            Mock.Of<IJwtTokenGenerator>());

        await Assert.ThrowsAsync<InvalidCredentialsException>(
            () => handler.Handle(new LoginCommand("ana@email.com", "senha123"), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_DeveLancarExcecao_QuandoSenhaIncorreta()
    {
        var user = User.Create("ana@email.com", "hash-correto", "Ana");

        var userRepositoryMock = new Mock<IUserRepository>();
        userRepositoryMock
            .Setup(r => r.GetUserByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var passwordHasherMock = new Mock<IPasswordHasher>();
        passwordHasherMock
            .Setup(h => h.Verify(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(false);

        var handler = new LoginCommandHandler(
            userRepositoryMock.Object,
            passwordHasherMock.Object,
            Mock.Of<IJwtTokenGenerator>());

        await Assert.ThrowsAsync<InvalidCredentialsException>(
            () => handler.Handle(new LoginCommand("ana@email.com", "senha-errada"), CancellationToken.None));

        userRepositoryMock.Verify(r => r.UpdateUserAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
