using Moq;
using SocialScope.Application.Features.Auth.Logout;
using SocialScope.Application.Interfaces;
using SocialScope.Domain.Entities;

namespace SocialScope.UnitTests.Features.Auth.Logout;

public class LogoutCommandHandlerTests
{
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock = new();
    private readonly Mock<IRefreshTokenGenerator> _refreshTokenGeneratorMock = new();
    private readonly LogoutCommandHandler _handler;

    public LogoutCommandHandlerTests()
    {
        _refreshTokenGeneratorMock
            .Setup(g => g.Hash("token"))
            .Returns("hash");

        _handler = new LogoutCommandHandler(
            _refreshTokenRepositoryMock.Object,
            _refreshTokenGeneratorMock.Object);
    }

    [Fact]
    public async Task Handle_DeveRevogarToken_QuandoAtivo()
    {
        var storedToken = RefreshToken.Create(Guid.NewGuid(), "hash", DateTimeOffset.UtcNow.AddDays(7));

        _refreshTokenRepositoryMock
            .Setup(r => r.GetRefreshTokenByHashAsync("hash", It.IsAny<CancellationToken>()))
            .ReturnsAsync(storedToken);

        await _handler.Handle(new LogoutCommand("token"), CancellationToken.None);

        Assert.True(storedToken.IsRevoked);
        _refreshTokenRepositoryMock.Verify(
            r => r.UpdateRefreshTokenAsync(storedToken, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_NaoDeveFazerNada_QuandoTokenNaoExiste()
    {
        _refreshTokenRepositoryMock
            .Setup(r => r.GetRefreshTokenByHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((RefreshToken?)null);

        await _handler.Handle(new LogoutCommand("token"), CancellationToken.None);

        _refreshTokenRepositoryMock.Verify(
            r => r.UpdateRefreshTokenAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
