namespace Pharmacy.Shared.Dto.Delivery;

public record DeliveryDto(
    int Id,
    int OrderId,
    int UserAddressId,
    decimal Price,
    string? Comment,
    DateTime? DeliveryDate
);