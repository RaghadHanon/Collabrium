namespace Application.Common.Exceptions;

public class TenantNotFoundException : Exception
{
    public TenantNotFoundException(string message) : base(message)
    {
    }
}