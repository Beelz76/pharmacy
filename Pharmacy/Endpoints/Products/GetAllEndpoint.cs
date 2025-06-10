using FastEndpoints;
using Pharmacy.Extensions;
using Pharmacy.Services.Interfaces;
using Pharmacy.Shared.Dto.Product;
using Pharmacy.Shared.Enums;

namespace Pharmacy.Endpoints.Products;

public class GetAllEndpoint : Endpoint<ProductFilters>
{
    private readonly ILogger<GetAllEndpoint> _logger;
    private readonly IProductService _productService;
    public GetAllEndpoint(ILogger<GetAllEndpoint> logger, IProductService productService)
    {
        _logger = logger;
        _productService = productService;
    }

    public override void Configure()
    {
        Post("products/paginated");
        AllowAnonymous();
        Tags("Products");
        Summary(s => { s.Summary = "Получить товары"; }); 
    }

    public override async Task HandleAsync(ProductFilters filters, CancellationToken ct)
    {
        int pageNumber = Query<int>("pageNumber", isRequired: false) == 0 ? 1 : Query<int>("pageNumber", isRequired: false);
        int pageSize = Query<int>("pageSize", isRequired: false) == 0 ? 20 : Query<int>("pageSize", isRequired: false);
        string? sortBy = Query<string>("sortBy", isRequired: false);
        string? sortOrder = Query<string>("sortOrder", isRequired: false);
        string? search = Query<string>("search", isRequired: false);
     
        var userId = User.GetUserId();
        var role = User.GetUserRole();
        if (role != UserRoleEnum.User)
        {
            userId = null;
        }
        
        var parameters = new ProductParameters
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            SortBy = sortBy,
            SortOrder = sortOrder,
            CategoryIds = filters.CategoryIds,
            ManufacturerIds = filters.ManufacturerIds,
            Countries = filters.Countries,
            Search = search,
            PropertyFilters = filters.PropertyFilters,
            IsAvailable = filters.IsAvailable,
        };
        
        var result = await _productService.GetPaginatedProductsAsync(parameters, userId);
        if (result.IsSuccess)
        {
            await SendOkAsync(result.Value, ct);
        }
        else
        {
            await SendAsync(result.Error, (int)result.Error.StatusCode, ct);
        }
    }
}
