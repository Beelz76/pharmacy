using Pharmacy.Shared.Enums;

namespace Pharmacy.Shared.Dto;

public record UserDto(
    int Id,
    UserRoleEnum Role,
    string Email,
    string? PasswordHash,
    bool EmailVerified,
    string FirstName,
    string LastName,
    string? Patronymic,
    string? Phone);