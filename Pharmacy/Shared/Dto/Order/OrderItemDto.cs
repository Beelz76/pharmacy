namespace Pharmacy.Shared.Dto.Order;

public record OrderItemDto(
    int ProductId,
    string ProductName,
    int Quantity,
    decimal Price,
    string? ImageUrl
);