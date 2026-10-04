namespace SocialScope.Application.Exceptions;

public sealed class UserNotFoundException : Exception
{
    public UserNotFoundException()
        : base("Usuário não encontrado.")
    {
    }
}
