namespace Application.Common.Exceptions;

public class TenantNotActiveException : Exception
{
    public TenantNotActiveException(string message) : base(message)
    {
    }
}