namespace Pharmacy.Shared.Dto;

public record CreateDeliveryRequest(
    int OrderId,
    int UserAddressId,
    string? Comment,
    DateTime? DeliveryDate
);