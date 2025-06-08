namespace Pharmacy.Shared.Dto.Delivery;

public record UpdateDeliveryRequest(
    int OrderId,
    int UserAddressId,
    string? Comment,
    DateTime? DeliveryDate
);