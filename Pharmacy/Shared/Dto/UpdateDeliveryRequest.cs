namespace Pharmacy.Shared.Dto;

public record UpdateDeliveryRequest(
    int OrderId,
    int UserAddressId,
    string? Comment,
    DateTime? DeliveryDate
);