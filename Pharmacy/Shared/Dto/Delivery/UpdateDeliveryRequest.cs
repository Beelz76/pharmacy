namespace Pharmacy.Shared.Dto.Delivery;

public record UpdateDeliveryRequest(
    int OrderId,
    int UserAddressId,
    decimal Price,
    string? Comment,
    DateTime? DeliveryDate
);