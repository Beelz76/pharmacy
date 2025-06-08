using Pharmacy.Shared.Dto.Address;

namespace Pharmacy.Shared.Dto.Pharmacy;

public record PharmacyDto(
    int Id,
    string Name,
    string? Phone,
    AddressDto Address
);