namespace Application.Common.Exceptions;

public class MustChangePasswordException : Exception
{
    public MustChangePasswordException()
        : base("You must change your password before accessing this resource.") { }
}