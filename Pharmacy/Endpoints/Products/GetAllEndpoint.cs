using FastEndpoints;
using Pharmacy.Services.Interfaces;

namespace Pharmacy.Endpoints.Products;

public class GetAllEndpoint : EndpointWithoutRequest
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
        Get("products");
        AllowAnonymous();
        Tags("Products");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await SendOkAsync (ct);
    }
}