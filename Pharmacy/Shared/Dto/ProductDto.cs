namespace Pharmacy.Shared.Dto;

public record ProductDto(
    int Id,
    string Name,
    decimal Price,
    int StockQuantity,
    int CategoryId,
    string CategoryName,
    string CategoryDescription,
    int ManufacturerId,
    string ManufacturerName,
    string ManufacturerCountry,
    string Description,
    bool IsAvailable,
    bool IsPrescriptionRequired,
    DateTime? ExpirationDate,
    List<string>? Images,
    List<ProductPropertyDto> Properties
);