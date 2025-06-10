using Pharmacy.Shared.Enums;

namespace Pharmacy.Shared.Dto.Order;

public record OrderFilters(
    int? UserId,
    string? UserEmail,
    string? UserFullName,
    string? Number,
    string? PharmacyName,
    string? PharmacyCity,
    int? PharmacyId,
    OrderStatusEnum? Status,
    DateTime? FromDate,
    DateTime? ToDate,
    decimal? FromPrice,
    decimal? ToPrice
    );