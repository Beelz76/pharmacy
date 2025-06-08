using Pharmacy.Shared.Enums;

namespace Pharmacy.Shared.Dto;

public record OrderFilters(
    int? UserId,
    string? UserEmail,
    string? UserFullName,
    string? Number,
    string? PharmacyCity,
    int? PharmacyId,
    OrderStatusEnum? Status,
    DateTime? FromDate,
    DateTime? ToDate
    );