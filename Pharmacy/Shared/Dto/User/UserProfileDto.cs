using Pharmacy.Shared.Dto.Pharmacy;

namespace Pharmacy.Shared.Dto.User;

public record UserProfileDto(
    string Email,
    string FirstName,
    string LastName,
    string? Patronymic,
    string? Phone,
    PharmacyDto? Pharmacy
);
