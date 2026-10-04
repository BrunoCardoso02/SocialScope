using Moq;
using SocialScope.Application.Exceptions;
using SocialScope.Application.Features.Auth.GetCurrentUser;
using SocialScope.Application.Interfaces;
using SocialScope.Domain.Entities;

namespace SocialScope.UnitTests.Features.Auth.GetCurrentUser;

public class GetCurrentUserQueryHandlerTests
{
    [Fact]
    public async Task Handle_DeveRetornarDadosDoUsuario_QuandoExiste()
    {
        var user = User.Create("ana@email.com", "hash", "Ana");

        var userRepositoryMock = new Mock<IUserRepository>();
        userRepositoryMock
            .Setup(r => r.GetUserByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var handler = new GetCurrentUserQueryHandler(userRepositoryMock.Object);

        var response = await handler.Handle(new GetCurrentUserQuery(user.Id), CancellationToken.None);

        Assert.Equal(user.Id, response.Id);
        Assert.Equal("ana@email.com", response.Email);
        Assert.Equal("Ana", response.Name);
    }

    [Fact]
    public async Task Handle_DeveLancarExcecao_QuandoUsuarioNaoExiste()
    {
        var userRepositoryMock = new Mock<IUserRepository>();
        userRepositoryMock
            .Setup(r => r.GetUserByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var handler = new GetCurrentUserQueryHandler(userRepositoryMock.Object);

        await Assert.ThrowsAsync<UserNotFoundException>(
            () => handler.Handle(new GetCurrentUserQuery(Guid.NewGuid()), CancellationToken.None));
    }
}
