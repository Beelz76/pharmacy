using FastEndpoints;
using Pharmacy.Services.Interfaces;

namespace Pharmacy.Endpoints.ProductCategories;

public class GetByIdSubcategoriesEndpoint : EndpointWithoutRequest
{
    private readonly ILogger<GetByIdSubcategoriesEndpoint> _logger;
    private readonly IProductCategoryService _productCategoryService;
    public GetByIdSubcategoriesEndpoint(ILogger<GetByIdSubcategoriesEndpoint> logger, IProductCategoryService productCategoryService)
    {
        _logger = logger;
        _productCategoryService = productCategoryService;
    }

    public override void Configure()
    {
        Get("categories/{categoryId:int}/subcategories");
        //AllowAnonymous();
        Tags("ProductCategories");
        Summary(s => { s.Summary = "Получить подкатегории"; }); 
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var categoryId = Route<int>("categoryId");
        
        var result = await _productCategoryService.GetSubcategoriesAsync(categoryId);
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