using Pharmacy.Services.Interfaces;
using FastEndpoints;

namespace Pharmacy.Endpoints.Products;

public class DeleteEndpoint : EndpointWithoutRequest
{
    private readonly ILogger<DeleteEndpoint> _logger;
    private readonly IProductService _productService;
    public DeleteEndpoint(ILogger<DeleteEndpoint> logger, IProductService productService)
    {
        _logger = logger;
        _productService = productService;
    }

    public override void Configure()
    {
        Delete("products/{productId:int}");
        //Roles("Admin");
        Tags("Products");
        Summary(s => { s.Summary = "Удалить товар"; }); 
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var productId = Route<int>("productId");
        
        var result = await _productService.DeleteAsync(productId);
        if (result.IsSuccess)
        {
            await SendOkAsync (ct);
        }
        else
        {
            await SendAsync(result.Error, (int)result.Error.Code, ct);
        }
    }
}