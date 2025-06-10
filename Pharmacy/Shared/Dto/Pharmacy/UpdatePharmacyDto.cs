using Pharmacy.Shared.Dto.Address;

namespace Pharmacy.Shared.Dto.Pharmacy;

public record UpdatePharmacyDto(
    string Name,
    string? Phone,
    CreateAddressDto Address);