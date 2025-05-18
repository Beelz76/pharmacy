using FastEndpoints;

namespace Pharmacy.Shared.Dto;

public class ProductParameters
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public List<int>? CategoryIds { get; set; } = null;
    public List<int>? ManufacturerIds { get; set; } = null;
    public string? SortBy { get; set; } = null;
    public string? SortOrder { get; set; } = null;
    public string? Search { get; set; } = null;
    public bool? IsAvailable { get; set; }
    public bool? IsPrescriptionRequired { get; set; }
    public Dictionary<string, List<string>>? PropertyFilters { get; set; } = null;
}

public class ProductQuery
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SortBy { get; set; }
    public string? SortOrder { get; set; }
}

public class ProductFilters
{
    public bool? IsAvailable { get; set; }
    public bool? IsPrescriptionRequired { get; set; }
    public List<int>? CategoryIds { get; set; }
    public List<int>? ManufacturerIds { get; set; }
    public Dictionary<string, List<string>>? PropertyFilters { get; set; }
}