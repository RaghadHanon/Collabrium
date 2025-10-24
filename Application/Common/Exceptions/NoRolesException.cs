namespace Application.Common.Exceptions;

public class NoRolesException : Exception
{
    public NoRolesException(string message) : base(message)
    {
    }
}