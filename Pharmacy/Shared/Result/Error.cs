using System.Net;
using Pharmacy.Shared.Enums;

namespace Pharmacy.Shared.Result;

public sealed record Error
{
    public static readonly Error None = new(HttpStatusCode.OK, ErrorTypeEnum.None, string.Empty);
    public static readonly Error NullValue = new(HttpStatusCode.BadRequest, ErrorTypeEnum.Failure, "Null value was provided");
    
    public Error(HttpStatusCode statusCode, ErrorTypeEnum type, string message, List<string>? details = null)
    {
        StatusCode = statusCode;
        Type = type;
        Message = message;
        Details = details;
    }

    public HttpStatusCode StatusCode { get; }
    public ErrorTypeEnum Type { get; }
    public string? Message { get; }
    public List<string>? Details { get; }

    public static Error Failure(string message, List<string>? details = null) => new(HttpStatusCode.BadRequest, ErrorTypeEnum.Failure, message, details);

    public static Error NotFound(string message) => new(HttpStatusCode.NotFound, ErrorTypeEnum.NotFound, message);
    public static Error Conflict(string message) => new(HttpStatusCode.Conflict, ErrorTypeEnum.Conflict, message);
    public static Error Problem(string message) => new(HttpStatusCode.InternalServerError, ErrorTypeEnum.Problem, message);
    public static Error Unauthorized(string message) => new(HttpStatusCode.Unauthorized, ErrorTypeEnum.Unauthorized, message);
}