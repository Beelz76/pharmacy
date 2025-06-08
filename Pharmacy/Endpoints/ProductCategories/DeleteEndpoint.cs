using FastEndpoints;
using Pharmacy.Services.Interfaces;

namespace Pharmacy.Endpoints.ProductCategories;

public class DeleteEndpoint : EndpointWithoutRequest
{
    private readonly ILogger<DeleteEndpoint> _logger;
    private readonly IProductCategoryService _productCategoryService;
    public DeleteEndpoint(ILogger<DeleteEndpoint> logger, IProductCategoryService productCategoryService)
    {
        _logger = logger;
        _productCategoryService = productCategoryService;
    }

    public override void Configure()
    {
        Delete("categories/{categoryId:int}");
        //Roles("Admin");
        Tags("ProductCategories");
        Summary(s => { s.Summary = "Удаление категории товаров"; }); 
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        int categoryId = Route<int>("categoryId");
        
        var result = await _productCategoryService.DeleteAsync(categoryId);
        if (result.IsSuccess)
        {
            await SendOkAsync (ct);
        }
        else
        {
            await SendAsync(result.Error, (int)result.Error.StatusCode, ct);
        }
    }
}