namespace Pharmacy.Shared.Dto;

public record FavoriteItemDto(
    int ProductId,
    string Name,
    string Description,
    string ManufacturerName,
    string ManufacturerCountry,
    decimal Price,
    string? ImageUrl,
    bool IsAvailable,
    bool IsPrescriptionRequired,
    int QuantityInCart);