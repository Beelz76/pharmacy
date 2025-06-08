namespace Pharmacy.Shared.Dto.Delivery;

public record DeliveryDto(
    int Id,
    int OrderId,
    int UserAddressId,
    string? Comment,
    DateTime? DeliveryDate
);