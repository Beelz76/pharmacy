using FastEndpoints;
using Pharmacy.Services.Interfaces;

namespace Pharmacy.Endpoints.ProductImages;

public class UploadEndpoint : EndpointWithoutRequest
{
    private readonly ILogger<UploadEndpoint> _logger;
    private readonly IProductImageService _productImageService;
    public UploadEndpoint(ILogger<UploadEndpoint> logger, IProductImageService productImageService)
    {
        _logger = logger;
        _productImageService = productImageService;
    }

    public override void Configure()
    {
        Post("products/{productId:int}/upload-images");
        AllowFileUploads();
        Roles("Admin");
        Tags("Products");
        Summary(s => { s.Summary = "Загрузка изображений товара"; }); 
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var productId = Route<int>("productId");
        if (Files.Count == 0)
        {
            await SendAsync("Файл обязателен", 400, ct);
            return;
        }
        
        var result = await _productImageService.UploadImagesAsync(productId, Files);
        if (result.IsSuccess)
        {
            await SendOkAsync (new { urls = result.Value }, ct);
        }
        else
        {
            await SendAsync(result.Error, (int)result.Error.Code, ct);
        }
    }
}