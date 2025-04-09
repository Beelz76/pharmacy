using FastEndpoints;
using Pharmacy.Services.Interfaces;

namespace Pharmacy.Endpoints.Products;

public class UpdateEndpoint : Endpoint<UpdateProductRequest>
{
    private readonly ILogger<UpdateEndpoint> _logger;
    private readonly IProductService _productService;
    public UpdateEndpoint(ILogger<UpdateEndpoint> logger, IProductService productService)
    {
        _logger = logger;
        _productService = productService;
    }

    public override void Configure()
    {
        Put("products/{id:int}");
        AllowAnonymous();
        Tags("Products");
    }

    public override async Task HandleAsync(UpdateProductRequest request, CancellationToken ct)
    {
        await SendOkAsync (ct);
    }
}

public record UpdateProductRequest(string name);