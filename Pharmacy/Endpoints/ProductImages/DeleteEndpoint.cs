using FastEndpoints;
using FluentValidation;
using Pharmacy.Services.Interfaces;

namespace Pharmacy.Endpoints.ProductImages;

public class DeleteMultipleEndpoint : Endpoint<DeleteMultipleImagesRequest>
{
    private readonly ILogger<DeleteMultipleEndpoint> _logger;
    private readonly IProductImageService _productImageService;

    public DeleteMultipleEndpoint(ILogger<DeleteMultipleEndpoint> logger, IProductImageService productImageService)
    {
        _logger = logger;
        _productImageService = productImageService;
    }

    public override void Configure()
    {
        Delete("products/{productId:int}/images");
        Roles("Admin");
        Tags("Products");
        Summary(s => { s.Summary = "Удалить несколько изображений товара"; });
    }

    public override async Task HandleAsync(DeleteMultipleImagesRequest req, CancellationToken ct)
    {
        var productId = Route<int>("productId");
        var result = await _productImageService.DeleteProductImagesAsync(productId, req.ImageIds);

        if (result.IsSuccess)
        {
            await SendOkAsync(ct);
        }
        else
        {
            await SendAsync(result.Error, (int)result.Error.Code, ct);
        }
    }
}

public record DeleteMultipleImagesRequest(List<int> ImageIds);

public class DeleteMultipleImagesRequestValidator : Validator<DeleteMultipleImagesRequest>
{
    public DeleteMultipleImagesRequestValidator()
    {
        RuleFor(x => x.ImageIds)
            .NotEmpty()
            .WithMessage("Список ID изображений не может быть пустым.");
    }
}
