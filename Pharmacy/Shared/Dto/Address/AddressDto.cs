namespace Pharmacy.Shared.Dto.Address;

public record AddressDto(
    int Id,
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