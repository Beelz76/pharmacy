namespace Pharmacy.Shared.Dto.Product;

public record ProductCategoryDto(
    int Id, 
    string Name, 
    string Description);

public record ProductCategoryNullableDto(
    int? Id, 
    string? Name, 
    string? Description);