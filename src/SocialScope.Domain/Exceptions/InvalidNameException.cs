namespace SocialScope.Domain.Exceptions;

public sealed class InvalidNameException : Exception
{
    public InvalidNameException()
        : base("O nome é obrigatório.")
    {
    }
}
