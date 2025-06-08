using Pharmacy.Shared.Enums;

namespace Pharmacy.Shared.Dto.User;

public record CreateUserDto(
    string Email,
    string PasswordHash,
    bool EmailVerified,
    string FirstName,
    string LastName,
    string? Patronymic,
    string? Phone,
    UserRoleEnum Role,
    int? PharmacyId);