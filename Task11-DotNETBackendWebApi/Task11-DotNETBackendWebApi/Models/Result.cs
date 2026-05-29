namespace Task11_DotNETBackendWebApi.Models;

public class Result<T>
{
    public T? Data { get; init; }
    public List<string> Errors { get; init; } = new();

    public static Result<T> Success(T data)
    {
        return new Result<T>
        {
            Data = data
        };
    }

    public static Result<T> Failure(string error)
    {
        return new Result<T>
        {
            Errors = new List<string> { error }
        };
    }

    public static Result<T> Failure(List<string> errors)
    {
        return new Result<T>
        {
            Errors = errors
        };
    }
}
