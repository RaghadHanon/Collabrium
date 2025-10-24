namespace Application.Common.Exceptions;

/// <summary>
/// Represents an error that occurs when authentication fails due to invalid credentials.
/// Maps to HTTP 401 Unauthorized.
/// </summary>
public class UnauthenticatedException : Exception
{
    public UnauthenticatedException(string message) : base(message)
    {
    }
}