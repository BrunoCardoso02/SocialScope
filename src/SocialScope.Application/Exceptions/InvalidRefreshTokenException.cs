namespace SocialScope.Application.Exceptions;

public sealed class InvalidRefreshTokenException : Exception
{
    public InvalidRefreshTokenException()
        : base("Refresh token inválido ou expirado.")
    {
    }
}
