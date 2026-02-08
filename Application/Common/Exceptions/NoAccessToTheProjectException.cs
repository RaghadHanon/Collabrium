namespace Application.Common.Exceptions;

public class NoAccessToTheProjectException : Exception
{
    public NoAccessToTheProjectException()
        : base("You do not have access to this project, join it's team first.") { }
}
