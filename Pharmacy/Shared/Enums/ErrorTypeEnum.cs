namespace Pharmacy.Shared.Enums;

public enum ErrorTypeEnum
{
    None = 0,
    Failure = 1,
    NotFound = 2,
    Validation = 3,
    Conflict = 4,
    Problem = 5,
    InternalServerError = 6,
    Unauthorized = 7,
    AccessForbidden = 8
}