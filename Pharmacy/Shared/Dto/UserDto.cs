namespace Pharmacy.Shared.Dto;

public record UserDto(
    int Id,
    string Email,
    string PasswordHash,
    bool EmailVerified,
    string FirstName,
    string LastName,
    string? Patronymic,
    string? Phone);