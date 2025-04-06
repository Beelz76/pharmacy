namespace Pharmacy.Models.Dtos;

public record UserDto(
    int Id,
    string Email,
    bool EmailVerified,
    string FirstName,
    string LastName,
    string? Patronymic,
    string? Phone);