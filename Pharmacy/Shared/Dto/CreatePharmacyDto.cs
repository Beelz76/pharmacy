namespace Pharmacy.Shared.Dto;

public record CreatePharmacyDto(
    string Name,
    string? Phone,
    CreateAddressDto Address
);