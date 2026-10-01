using SocialScope.Domain.Exceptions;
using SocialScope.Domain.ValueObjects;

namespace SocialScope.UnitTests.ValueObjects;

public class EmailTests
{
    [Theory]
    [InlineData("ana@email.com")]
    [InlineData("ana.silva@sub.dominio.com")]
    public void Create_DeveCriarEmail_QuandoFormatoValido(string value)
    {
        var email = Email.Create(value);

        Assert.Equal(value, email.Value);
    }

    [Theory]
    [InlineData("ana")]
    [InlineData("ana@")]
    [InlineData("@email.com")]
    [InlineData("ana email.com")]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_DeveLancarExcecao_QuandoFormatoInvalido(string value)
    {
        Assert.Throws<InvalidEmailException>(() => Email.Create(value));
    }
}
