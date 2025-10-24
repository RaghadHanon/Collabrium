namespace Application.Common.Exceptions;

public class TokenGenerationFailedException : Exception
{
    public TokenGenerationFailedException(string message) : base(message)
    {
    }
}