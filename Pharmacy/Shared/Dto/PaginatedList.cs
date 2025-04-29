namespace Pharmacy.Shared.Dto;

public record PaginatedList<T>(
    IEnumerable<T> Items,
    int TotalCount,
    int PageNumber,
    int PageSize
);