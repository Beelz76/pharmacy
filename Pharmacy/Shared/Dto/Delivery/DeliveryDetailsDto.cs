namespace Pharmacy.Shared.Dto.Delivery;

public record DeliveryDetailsDto(
    int Id,
    int OrderId,
    string OrderNumber,
    string Address,
    decimal Price,
    string? Comment,
    DateTime? DeliveryDate);