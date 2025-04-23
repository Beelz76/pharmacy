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
        await SendOkAsync (ct);
    }
}