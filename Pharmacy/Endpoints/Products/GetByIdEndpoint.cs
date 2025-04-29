using FastEndpoints;
using Pharmacy.Services.Interfaces;

namespace Pharmacy.Endpoints.Products;

public class GetByIdEndpoint : EndpointWithoutRequest
{
    private readonly ILogger<GetByIdEndpoint> _logger;
    private readonly IProductService _productService;
    public GetByIdEndpoint(ILogger<GetByIdEndpoint> logger, IProductService productService)
    {
        _logger = logger;
        _productService = productService;
    }

    public override void Configure()
    {
        Get("products/{id:int}");
        AllowAnonymous();
        Tags("Products");
        Summary(s => { s.Summary = "Получить товар по id"; }); 
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var productId = Route<int>("id");
        
        var result = await _productService.GetByIdAsync(productId);
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