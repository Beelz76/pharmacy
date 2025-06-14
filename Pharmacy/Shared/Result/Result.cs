using System.Net;
using Pharmacy.Shared.Enums;

namespace Pharmacy.Shared.Result;

public class Result
{
    protected Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None || !isSuccess && error == Error.None)
        {
            throw new ArgumentException("Недопустимое состояние ошибки", nameof(error));
        }

        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }

    public static Result Success() => new(true, Error.None);

    public static Result Failure(HttpStatusCode statusCode, ErrorTypeEnum errorType, string message) =>
        new(false, new Error(statusCode, errorType, message));

    public static Result Failure(Error error) => new(false, error);

    public static Result<T> Success<T>(T value) => new(value, true, Error.None);

    public static Result<T> Failure<T>(Error error) => new(default, false, error);
}

public class Result<T> : Result
{
    private readonly T? _value;

    public Result(T? value, bool isSuccess, Error error) : base(isSuccess, error)
    {
        _value = value;
    }
    
    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Невозможно получить доступ к значению при сбое операции");
    
    public T? ValueOrDefault => _value;

    public static implicit operator Result<T>(T? value) =>
        value is not null ? Success(value) : Failure<T>(Error.NullValue);
}