namespace Pharmacy.Shared.Dto;

public record ProductCategoryDto(
    int Id, 
    string Name, 
    string Description);

public record ProductCategoryNullableDto(
    int? Id, 
    string? Name, 
    string? Description);