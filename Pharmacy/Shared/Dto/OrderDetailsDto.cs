namespace Pharmacy.Shared.Dto;

public record OrderDetailsDto(
    int Id,
    string Number,
    DateTime CreatedAt,
    decimal TotalPrice,
    string Status,
    string? PickupCode,
    string? PharmacyName,
    string? PharmacyAddress,
    string? DeliveryAddress,
    bool IsDelivery,
    int UserId,
    string UserFullName,
    string UserEmail,
    List<OrderItemDto> Items,
    PaymentDto Payment
);