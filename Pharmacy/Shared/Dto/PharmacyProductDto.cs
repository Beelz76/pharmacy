namespace Pharmacy.Shared.Dto;

public record PharmacyProductDto(
    int ProductId,
    string ProductName,
    int StockQuantity,
    decimal Price,
    bool IsAvailable
);