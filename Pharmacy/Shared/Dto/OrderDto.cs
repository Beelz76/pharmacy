namespace Pharmacy.Shared.Dto;

public record OrderDto(
    int Id,
    string Number,
    DateTime CreatedAt,
    decimal TotalPrice,
    string Status,
    string? PickupCode,
    int PharmacyId,
    string? PharmacyName,
    string PharmacyAddress,
    int UserId,
    string UserFullName,
    string UserEmail
);