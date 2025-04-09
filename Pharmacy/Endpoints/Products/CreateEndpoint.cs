using FastEndpoints;
using Pharmacy.Services.Interfaces;

namespace Pharmacy.Endpoints.Products;

public class CreateEndpoint : Endpoint<CreateProductRequest>
{
    private readonly ILogger<CreateEndpoint> _logger;
    private readonly IProductService _productService;
    public CreateEndpoint(ILogger<CreateEndpoint> logger, IProductService productService)
    {
        _logger = logger;
        _productService = productService;
    }

    public override void Configure()
    {
        Post("products");
        AllowAnonymous();
        Tags("Products");
    }

    public override async Task HandleAsync(CreateProductRequest request, CancellationToken ct)
    {
        await SendOkAsync (ct);
    }
}

public record CreateProductRequest(string name);