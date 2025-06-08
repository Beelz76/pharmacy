namespace Pharmacy.Shared.Dto.Delivery;

public record CreateDeliveryRequest(
    int OrderId,
    int UserAddressId,
    string? Comment,
    DateTime? DeliveryDate
);