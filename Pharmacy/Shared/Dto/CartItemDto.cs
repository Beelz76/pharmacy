namespace Pharmacy.Shared.Dto;

public record CartItemDto(
    int ProductId,
    string Name,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice,
    string? ImageUrl,
    bool IsAvailable
);
