namespace Task11_DotNETBackendWebApi.Models;

public class Result
{
    public bool IsSuccess { get; init; }
    public List<string> Errors { get; init; } = new();

    public static Result Success()
    {
        return new Result
        {
            IsSuccess = true
        };
    }

    public static Result Failure(string error)
    {
        return new Result
        {
            IsSuccess = false,
            Errors = new List<string> { error }
        };
    }

    public static Result Failure(List<string> errors)
    {
        return new Result
        {
            IsSuccess = false,
            Errors = errors
        };
    }
}

public class Result<T> : Result
{
    public T? Data { get; init; }

    public static Result<T> Success(T data)
    {
        return new Result<T>
        {
            IsSuccess = true,
            Data = data
        };
    }

    public new static Result<T> Failure(string error)
    {
        return new Result<T>
        {
            IsSuccess = false,
            Errors = new List<string> { error }
        };
    }

    public new static Result<T> Failure(List<string> errors)
    {
        return new Result<T>
        {
            IsSuccess = false,
            Errors = errors
        };
    }
}
