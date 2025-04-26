namespace QuickDirTree;

public enum ResultStatus
{
    Ok,
    Ng,
    Cancel
}

public readonly struct Unit
{
    public static readonly Unit Value = new Unit();
}

public static class Results {
    public static Result<T> Ok<T>(T value) => Result<T>.Ok(value);
    public static Result<T> Ng<T>(string? message = null) => Result<T>.Ng(message);
    public static Result<T> Cancel<T>(string? message = null) => Result<T>.Cancel(message);
}

public class Result<T>
{
    public ResultStatus Status { get; }
    public T? Value { get; }
    public string? Message { get; }

    private Result(ResultStatus status, T? value = default, string? message = null)
    {
        Status = status;
        Value = value;
        Message = message;
    }

    public static Result<T> Ok(T value) => new Result<T>(ResultStatus.Ok, value);
    public static Result<T> Ng(string? message = null) => new Result<T>(ResultStatus.Ng, default, message);
    public static Result<T> Cancel(string? message = null) => new Result<T>(ResultStatus.Cancel, default, message);

    public bool IsOk => Status == ResultStatus.Ok;
    public bool IsNg => Status == ResultStatus.Ng;
    public bool IsCancel => Status == ResultStatus.Cancel;

    public Result<U> Bind<U>(Func<T, Result<U>> binder)
    {
        if (IsOk && Value != null)
        {
            return binder(Value);
        }
        else if (IsCancel)
        {
            return Result<U>.Cancel(Message);
        }
        else
        {
            return Result<U>.Ng(Message);
        }
    }
    public Result<U> Map<U>(Func<T, U> mapper)
    {
        if (IsOk && Value != null)
        {
            return Result<U>.Ok(mapper(Value));
        }
        else if (IsCancel)
        {
            return Result<U>.Cancel(Message);
        }
        else
        {
            return Result<U>.Ng(Message);
        }
    }
}