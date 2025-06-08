using FastEndpoints;
using Pharmacy.Services.Interfaces;

namespace Pharmacy.Endpoints.ProductCategories;

public class GetFieldsEndpoint : EndpointWithoutRequest
{
    private readonly ILogger<GetFieldsEndpoint> _logger;
    private readonly IProductCategoryService _productCategoryService;
    public GetFieldsEndpoint(ILogger<GetFieldsEndpoint> logger, IProductCategoryService productCategoryService)
    {
        _logger = logger;
        _productCategoryService = productCategoryService;
    }

    public override void Configure()
    {
        Get("categories/{categoryId:int}/fields");
        //Roles("Admin", "User");
        Tags("ProductCategories");
        Summary(s => { s.Summary = "Получение полей для категории товаров"; }); 
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        int categoryId = Route<int>("categoryId");
        
        var result = await _productCategoryService.GetAllFieldsIncludingParentAsync(categoryId, true);
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