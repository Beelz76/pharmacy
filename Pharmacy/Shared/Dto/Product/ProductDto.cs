using Pharmacy.Shared.Dto.Manufacturer;

namespace Pharmacy.Shared.Dto.Product;

public record ProductDto(
    int Id,
    string Sku,
    string Name,
    decimal Price,
    ProductCategoryDto Category,
    ProductCategoryNullableDto ParentCategory,
    ManufacturerDto Manufacturer,
    string Description,
    string ExtendedDescription,
    bool IsAvailable,
    bool IsPrescriptionRequired,
    DateTime? ExpirationDate,
    List<string>? Images,
    List<ProductPropertyDto> Properties
);