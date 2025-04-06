using System.Net;
using Pharmacy.Models.Enums;

namespace Pharmacy.Models.Result;

public sealed record Error(HttpStatusCode Code, ErrorTypeEnum ErrorType, string? Message = null);