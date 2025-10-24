namespace Application.Common.Exceptions;

public class MissingTenantIdException : Exception
{
    public MissingTenantIdException(string message) : base(message)
    {
    }
}