namespace Pharmacy.Shared.Dto;

public record FavoriteItemDto(
    int ProductId,
    string Name,
    decimal Price,
    string? ImageUrl,
    bool IsAvailable,
    int QuantityInCart);