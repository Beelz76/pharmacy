namespace Pharmacy.Shared.Dto.Delivery;

public record DeliveryFilters(
    string? OrderNumber,
    int? PharmacyId,
    DateTime? FromDate,
    DateTime? ToDate);