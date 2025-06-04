namespace Pharmacy.Shared.Dto;

public record ProductCardDto(
    int Id,
    string Name,
    string Description,
    decimal Price,
    string? ImageUrl,
    bool IsGloballyDisabled,
    bool IsPrescriptionRequired,
    bool IsFavorite,
    int CartQuantity
);
// {
//     public bool IsFavorite { get; set; }
//     public int CartQuantity { get; set; }
// };