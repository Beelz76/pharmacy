namespace Pharmacy.Shared.Dto.Product;

public record ProductCardDto(
    int Id,
    string Name,
    string Description,
    decimal Price,
    string? ImageUrl,
    bool IsAvailable,
    bool IsFavorite,
    int CartQuantity,
    int CategoryId,
    string CategoryName
);