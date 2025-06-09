using FastEndpoints;
using Pharmacy.Services.Interfaces;

namespace Pharmacy.Endpoints.Products;

public class GetBySkuEndpoint : EndpointWithoutRequest
{
    private readonly ILogger<GetBySkuEndpoint> _logger;
    private readonly IProductService _productService;

    public GetBySkuEndpoint(ILogger<GetBySkuEndpoint> logger, IProductService productService)
    {
        _logger = logger;
        _productService = productService;
    }

    public override void Configure()
    {
        Get("products/by-sku/{sku}");
        AllowAnonymous();
        Tags("Products");
        Summary(s => { s.Summary = "Получить товар по артикулу"; });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var sku = Route<string>("sku");
        var result = await _productService.GetBySkuAsync(sku);
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