using Pharmacy.Shared.Dto.Payment;

namespace Pharmacy.Shared.Dto.Order;

public record OrderDetailsDto(
    int Id,
    string Number,
    DateTime CreatedAt,
    decimal TotalPrice,
    decimal DeliveryPrice,
    decimal FinalPrice,
    string Status,
    string? PickupCode,
    string? PharmacyName,
    string? PharmacyAddress,
    int PharmacyId,
    string? DeliveryAddress,
    bool IsDelivery,
    int UserId,
    string UserFullName,
    string UserEmail,
    List<OrderItemDto> Items,
    PaymentDto Payment,
    string? CancellationComment,
    DateTime? ExpiresAt,
    bool RepeatAvailable
);