using FastEndpoints;
using Pharmacy.Services.Interfaces;

namespace Pharmacy.Endpoints.ProductImages;

public class DeleteEndpoint : EndpointWithoutRequest
{
    private readonly ILogger<DeleteEndpoint> _logger;
    private readonly IProductImageService _productImageService;
    public DeleteEndpoint(ILogger<DeleteEndpoint> logger, IProductImageService productImageService)
    {
        _logger = logger;
        _productImageService = productImageService;
    }

    public override void Configure()
    {
        Delete("products/{productId:int}/images/{imageId:int}");
        Roles("Admin");
        Tags("Products");
        Summary(s => { s.Summary = "Удалить изображение товара"; }); 
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var productId = Route<int>("productId");
        var imageId = Route<int>("imageId");
        
        var result = await _productImageService.DeleteProductImageAsync(productId, imageId);
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