namespace Pharmacy.Shared.Dto;

public record UserProfileDto(
    string Email,
    string FirstName,
    string LastName,
    string? Patronymic,
    string? Phone,
    PharmacyDto? Pharmacy
);
