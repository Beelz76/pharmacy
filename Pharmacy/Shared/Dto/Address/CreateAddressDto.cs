namespace Pharmacy.Shared.Dto.Address;

public record CreateAddressDto(
    string? OsmId,
    string? Region,
    string? State,
    string? City,
    string? Suburb,
    string? Street,
    string? HouseNumber,
    string? Postcode,
    double Latitude,
    double Longitude
);