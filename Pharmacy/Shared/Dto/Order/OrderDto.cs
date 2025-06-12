namespace Pharmacy.Shared.Dto.Order;

public record OrderDto(
    int Id,
    string Number,
    DateTime CreatedAt,
    decimal TotalPrice,
    string Status,
    string? PickupCode,
    bool IsDelivery,
    int PharmacyId,
    string? PharmacyName,
    string PharmacyAddress,
    int UserId,
    string UserFullName,
    string UserEmail,
    string? CancellationComment,
    string PaymentStatus
);