namespace Pharmacy.Shared.Dto.Category;

public record CategoryFieldDto(
    int? Id,
    string Key,
    string Label,
    string Type,
    bool IsRequired,
    bool IsFilterable
);