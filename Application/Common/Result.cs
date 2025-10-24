using System.Net;

namespace Application.Common;

public class Result<T>
{
    public bool IsSuccess { get; set; }
    public string? Error { get; set; }
    public T? Value { get; set; }
    public HttpStatusCode StatusCode { get; set; }

    private Result(bool isSuccess, T? value, string? error, HttpStatusCode statusCode)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
        StatusCode = statusCode;
    }

    public static Result<T> Success(T? value, HttpStatusCode statusCode)
    {
        return new(true, value, null, statusCode);
    }

    public static Result<T> Failure(string? error, HttpStatusCode statusCode)
    {
        return new(false, default, error, statusCode);
    }
}