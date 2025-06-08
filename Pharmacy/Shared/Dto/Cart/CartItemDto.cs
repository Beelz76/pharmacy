namespace Pharmacy.Shared.Dto.Cart;

public record CartItemDto(
    int ProductId,
    string Name,
    string Description,
    string ManufacturerName,
    string ManufacturerCountry,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice,
    string? ImageUrl,
    bool IsAvailable
);
