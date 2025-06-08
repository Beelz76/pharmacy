namespace Pharmacy.Shared.Dto.Delivery;

public record DeliveryDetailsDto(
    int Id,
    int OrderId,
    string OrderNumber,
    string Address,
    string? Comment,
    DateTime? DeliveryDate);