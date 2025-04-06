using System.ComponentModel;

namespace Pharmacy.Models.Enums;

public enum ErrorTypeEnum
{
    Failure = 0,
    NotFound = 1,
    Validation = 2,
    Conflict = 3,
    InternalServerError = 4,
    AccessUnAuthorized = 5,
    AccessForbidden = 6
}