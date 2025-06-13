namespace Pharmacy.Shared.Dto.Delivery;

public record CreateDeliveryRequest(
    int OrderId,
    int UserAddressId,
    decimal Price,
    string? Comment,
    DateTime? DeliveryDate
);