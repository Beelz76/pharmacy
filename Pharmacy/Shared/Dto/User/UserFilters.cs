using Pharmacy.Shared.Enums;

namespace Pharmacy.Shared.Dto.User;

public record UserFilters(
    string? FirstName,
    string? LastName,
    string? Patronymic,
    UserRoleEnum? Role,
    string? Email);