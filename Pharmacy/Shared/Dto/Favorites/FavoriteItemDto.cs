namespace Pharmacy.Shared.Dto.Favorites;

public record FavoriteItemDto(
    int ProductId,
    string Name,
    string Description,
    string ManufacturerName,
    string ManufacturerCountry,
    decimal Price,
    string? ImageUrl,
    bool IsAvailable,
    int QuantityInCart);