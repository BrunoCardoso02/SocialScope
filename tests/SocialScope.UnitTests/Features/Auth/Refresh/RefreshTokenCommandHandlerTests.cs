using Moq;
using SocialScope.Application.Exceptions;
using SocialScope.Application.Features.Auth.Refresh;
using SocialScope.Application.Interfaces;
using SocialScope.Domain.Entities;

namespace SocialScope.UnitTests.Features.Auth.Refresh;

public class RefreshTokenCommandHandlerTests
{
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock = new();
    private readonly Mock<IRefreshTokenGenerator> _refreshTokenGeneratorMock = new();
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IJwtTokenGenerator> _jwtTokenGeneratorMock = new();
    private readonly RefreshTokenCommandHandler _handler;

    public RefreshTokenCommandHandlerTests()
    {
        _refreshTokenGeneratorMock
            .Setup(g => g.Hash("token-antigo"))
            .Returns("hash-antigo");

        _handler = new RefreshTokenCommandHandler(
            _refreshTokenRepositoryMock.Object,
            _refreshTokenGeneratorMock.Object,
            _userRepositoryMock.Object,
            _jwtTokenGeneratorMock.Object);
    }

    [Fact]
    public async Task Handle_DeveRotacionarToken_QuandoRefreshTokenValido()
    {
        var user = User.Create("ana@email.com", "hash", "Ana");
        var storedToken = RefreshToken.Create(user.Id, "hash-antigo", DateTimeOffset.UtcNow.AddDays(7));
        var newExpiresAt = DateTimeOffset.UtcNow.AddDays(7);
        var accessExpiresAt = DateTimeOffset.UtcNow.AddMinutes(15);

        _refreshTokenRepositoryMock
            .Setup(r => r.GetRefreshTokenByHashAsync("hash-antigo", It.IsAny<CancellationToken>()))
            .ReturnsAsync(storedToken);
        _userRepositoryMock
            .Setup(r => r.GetUserByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _refreshTokenGeneratorMock
            .Setup(g => g.Generate())
            .Returns(new GeneratedRefreshToken("token-novo", "hash-novo", newExpiresAt));
        _jwtTokenGeneratorMock
            .Setup(g => g.Generate(user))
            .Returns(("access-novo", accessExpiresAt));

        var response = await _handler.Handle(new RefreshTokenCommand("token-antigo"), CancellationToken.None);

        Assert.Equal("access-novo", response.AccessToken);
        Assert.Equal("token-novo", response.RefreshToken);
        Assert.Equal(newExpiresAt, response.RefreshTokenExpiresAt);
        Assert.True(storedToken.IsRevoked);
        _refreshTokenRepositoryMock.Verify(
            r => r.UpdateRefreshTokenAsync(storedToken, It.IsAny<CancellationToken>()),
            Times.Once);
        _refreshTokenRepositoryMock.Verify(
            r => r.AddRefreshTokenAsync(
                It.Is<RefreshToken>(t => t.UserId == user.Id && t.TokenHash == "hash-novo"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_DeveLancarExcecao_QuandoTokenNaoExiste()
    {
        _refreshTokenRepositoryMock
            .Setup(r => r.GetRefreshTokenByHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((RefreshToken?)null);

        await Assert.ThrowsAsync<InvalidRefreshTokenException>(
            () => _handler.Handle(new RefreshTokenCommand("token-antigo"), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_DeveLancarExcecao_QuandoTokenVazio()
    {
        await Assert.ThrowsAsync<InvalidRefreshTokenException>(
            () => _handler.Handle(new RefreshTokenCommand(" "), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_DeveLancarExcecao_QuandoTokenExpirado()
    {
        var storedToken = RefreshToken.Create(Guid.NewGuid(), "hash-antigo", DateTimeOffset.UtcNow.AddMinutes(-1));

        _refreshTokenRepositoryMock
            .Setup(r => r.GetRefreshTokenByHashAsync("hash-antigo", It.IsAny<CancellationToken>()))
            .ReturnsAsync(storedToken);

        await Assert.ThrowsAsync<InvalidRefreshTokenException>(
            () => _handler.Handle(new RefreshTokenCommand("token-antigo"), CancellationToken.None));

        _refreshTokenRepositoryMock.Verify(
            r => r.AddRefreshTokenAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_DeveRevogarTodosOsTokensDoUsuario_QuandoTokenJaRevogadoForReutilizado()
    {
        var userId = Guid.NewGuid();
        var storedToken = RefreshToken.Create(userId, "hash-antigo", DateTimeOffset.UtcNow.AddDays(7));
        storedToken.Revoke();

        _refreshTokenRepositoryMock
            .Setup(r => r.GetRefreshTokenByHashAsync("hash-antigo", It.IsAny<CancellationToken>()))
            .ReturnsAsync(storedToken);

        await Assert.ThrowsAsync<InvalidRefreshTokenException>(
            () => _handler.Handle(new RefreshTokenCommand("token-antigo"), CancellationToken.None));

        _refreshTokenRepositoryMock.Verify(
            r => r.RevokeAllUserRefreshTokensAsync(userId, It.IsAny<CancellationToken>()),
            Times.Once);
        _refreshTokenRepositoryMock.Verify(
            r => r.AddRefreshTokenAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
