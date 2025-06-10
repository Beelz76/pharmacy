using Pharmacy.Shared.Enums;

namespace Pharmacy.Shared.Dto.User;

public record UpdateUserDto(
    string Email,
    string FirstName,
    string LastName,
    string? Patronymic,
    string? Phone,
    UserRoleEnum Role,
    int? PharmacyId);