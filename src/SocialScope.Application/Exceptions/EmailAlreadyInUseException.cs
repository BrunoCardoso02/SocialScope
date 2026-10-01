namespace SocialScope.Application.Exceptions;

public sealed class EmailAlreadyInUseException : Exception
{
    public EmailAlreadyInUseException(string email)
        : base($"O e-mail '{email}' já está em uso.")
    {
    }
}
