namespace Pharmacy.Shared.Dto;

public record OrderItemDto(
    int ProductId,
    string ProductName,
    int Quantity,
    decimal Price,
    string? ImageUrl
);