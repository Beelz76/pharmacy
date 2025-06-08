using FastEndpoints;
using Pharmacy.Services.Interfaces;

namespace Pharmacy.Endpoints.Products;

public class GetFilterValuesEndpoint : EndpointWithoutRequest
{
    private readonly ILogger<GetFilterValuesEndpoint> _logger;
    private readonly IProductService _productService;
    public GetFilterValuesEndpoint(ILogger<GetFilterValuesEndpoint> logger, IProductService productService)
    {
        _logger = logger;
        _productService = productService;
    }

    public override void Configure()
    {
        Get("products/filter-values/{categoryId:int}");
        AllowAnonymous();
        Tags("Products");
        Summary(s => { s.Summary = "Получить возможные значения фильтров для категории"; }); 
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var categoryId = Route<int>("categoryId");
        
        var result = await _productService.GetFilterValuesAsync(categoryId);
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