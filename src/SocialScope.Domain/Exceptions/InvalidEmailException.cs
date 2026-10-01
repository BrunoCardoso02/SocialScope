namespace SocialScope.Domain.Exceptions;

public sealed class InvalidEmailException : Exception
{
    public InvalidEmailException(string email)
        : base($"O e-mail '{email}' é inválido.")
    {
    }
}
