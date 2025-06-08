using FastEndpoints;
using Pharmacy.Services.Interfaces;

namespace Pharmacy.Endpoints.ProductCategories;


public class GetAllEndpoint : EndpointWithoutRequest
{
    private readonly ILogger<GetAllEndpoint> _logger;
    private readonly IProductCategoryService _productCategoryService;
    public GetAllEndpoint(ILogger<GetAllEndpoint> logger, IProductCategoryService productCategoryService)
    {
        _logger = logger;
        _productCategoryService = productCategoryService;
    }

    public override void Configure()
    {
        Get("categories");
        AllowAnonymous();
        Tags("ProductCategories");
        Summary(s => { s.Summary = "Получение всех категорий товаров"; }); 
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await _productCategoryService.GetAllWithSubcategoriesAsync();
        if (result.IsSuccess)
        {
            await SendOkAsync (result.Value, ct);
        }
        else
        {
            await SendAsync(result.Error, (int)result.Error.StatusCode, ct);
        }
    }
}