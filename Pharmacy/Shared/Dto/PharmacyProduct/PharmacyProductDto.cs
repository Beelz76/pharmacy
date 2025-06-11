namespace Pharmacy.Shared.Dto.PharmacyProduct;

public record PharmacyProductDto(
    int ProductId,
    string ProductName,
    int StockQuantity,
    decimal Price,
    bool IsAvailable,
    bool IsGlobalPrice
);