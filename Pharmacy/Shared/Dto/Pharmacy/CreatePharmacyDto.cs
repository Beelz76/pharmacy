using Pharmacy.Shared.Dto.Address;

namespace Pharmacy.Shared.Dto.Pharmacy;

public record CreatePharmacyDto(
    string Name,
    string? Phone,
    CreateAddressDto Address
);