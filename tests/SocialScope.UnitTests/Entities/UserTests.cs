using SocialScope.Domain.Entities;
using SocialScope.Domain.Exceptions;

namespace SocialScope.UnitTests.Entities;

public class UserTests
{
    [Fact]
    public void Create_DeveCriarUsuario_ComNomeSemEspacosNasPontas()
    {
        var user = User.Create("ana@email.com", "hash", "  Ana  ");

        Assert.Equal("Ana", user.Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_DeveLancarExcecao_QuandoNomeVazio(string? name)
    {
        Assert.Throws<InvalidNameException>(() => User.Create("ana@email.com", "hash", name!));
    }
}
