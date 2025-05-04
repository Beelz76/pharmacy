using Pharmacy.Shared.Enums;

namespace Pharmacy.Shared.Dto;

public record OrderFilters(
    int? UserId,
    string? UserEmail,
    string? UserFullName,
    string? Number,
    OrderStatusEnum? Status,
    DateTime? FromDate,
    DateTime? ToDate,
    string? SortBy,
    string? SortOrder
    );