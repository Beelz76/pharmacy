namespace Pharmacy.Shared.Dto;

public record DeliveryDto(
    int Id,
    int OrderId,
    int UserAddressId,
    string? Comment,
    DateTime? DeliveryDate
);