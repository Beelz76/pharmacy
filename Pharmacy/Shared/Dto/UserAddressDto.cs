namespace Pharmacy.Shared.Dto;

public record UserAddressDto(
    int Id, 
    string City, 
    string Street, 
    string? House, 
    string? Apartment);