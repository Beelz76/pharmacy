using System.Net;
using Pharmacy.Models.Enums;

namespace Pharmacy.Models.Result;

public class Result
{
    protected Result()
    {
        IsSuccess = true;
        Error = default!;
    }

    protected Result(Error error)
    {
        IsSuccess = false;
        Error = error;
    }

    public bool IsSuccess { get; }
    public Error Error { get; }

    public static Result Success() => new();
    public static Result Failure(HttpStatusCode statusCode, ErrorTypeEnum errorType, string message)
    {
        return new Result(new Error(statusCode, errorType, message));
    }
}

public sealed class Result<TValue> : Result
{
    private readonly TValue? _value;

    private Result(TValue value) : base()
    {
        _value = value;
    }

    private Result(Error error) : base(error)
    {
        _value = default;
    }

    public TValue Value => IsSuccess ? _value! : throw new InvalidOperationException("Value can not be accessed when IsSuccess is false");

    public static Result<TValue> Success(TValue value) => new(value);
    public new static Result<TValue> Failure(HttpStatusCode statusCode, ErrorTypeEnum errorType, string message)
    {
        return new Result<TValue>(new Error(statusCode, errorType, message));
    }
}
