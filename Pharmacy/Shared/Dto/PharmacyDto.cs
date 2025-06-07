namespace Pharmacy.Shared.Dto;

public record PharmacyDto(
    int Id,
    string Name,
    string? Phone,
    AddressDto Address
);