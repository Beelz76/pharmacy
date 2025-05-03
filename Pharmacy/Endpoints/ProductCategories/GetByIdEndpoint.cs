using FastEndpoints;
using Pharmacy.Services.Interfaces;

namespace Pharmacy.Endpoints.ProductCategories;

public class GetByIdEndpoint : EndpointWithoutRequest
{
    private readonly ILogger<GetByIdEndpoint> _logger;
    private readonly IProductCategoryService _productCategoryService;
    public GetByIdEndpoint(ILogger<GetByIdEndpoint> logger, IProductCategoryService productCategoryService)
    {
        _logger = logger;
        _productCategoryService = productCategoryService;
    }

    public override void Configure()
    {
        Get("categories/{categoryId:int}");
        //AllowAnonymous();
        Tags("ProductCategories");
        Summary(s => { s.Summary = "Получить категорию по id"; }); 
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var categoryId = Route<int>("categoryId");
        
        var result = await _productCategoryService.GetWithSubcategoriesByIdAsync(categoryId);
        if (result.IsSuccess)
        {
            await SendOkAsync (result.Value, ct);
        }
        else
        {
            await SendAsync(result.Error, (int)result.Error.Code, ct);
        }
    }
}