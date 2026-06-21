
[Serializable]
internal class UserNotActiveException : Exception
{
    public UserNotActiveException()
    {
    }

    public UserNotActiveException(string? message) : base(message)
    {
    }

    public UserNotActiveException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}