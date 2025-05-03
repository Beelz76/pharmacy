namespace Pharmacy.Shared.Dto;

public record ProductCategoryWithSubDto(
    int Id,
    string Name,
    string Description,
    int? ParentCategoryId,
    List<ProductCategoryDto> Subcategories
);
