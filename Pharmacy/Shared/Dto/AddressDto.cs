namespace Pharmacy.Shared.Dto;

public record AddressDto(
    int Id,
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