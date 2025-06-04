using FastEndpoints;
using FluentValidation;
using Pharmacy.Services.Interfaces;

namespace Pharmacy.Endpoints.ProductImages;

public class AddExternalImagesEndpoint : Endpoint<AddExternalImagesRequest>
{
    private readonly ILogger<AddExternalImagesEndpoint> _logger;
    private readonly IProductImageService _productImageService;

    public AddExternalImagesEndpoint(ILogger<AddExternalImagesEndpoint> logger, IProductImageService productImageService)
    {
        _logger = logger;
        _productImageService = productImageService;
    }

    public override void Configure()
    {
        Post("products/{productId:int}/add-external-images");
        Roles("Admin");
        Tags("Products");
        Summary(s => { s.Summary = "Добавление внешних ссылок на изображения товара"; });
    }

    public override async Task HandleAsync(AddExternalImagesRequest req, CancellationToken ct)
    {
        var productId = Route<int>("productId");
        var result = await _productImageService.AddExternalImagesAsync(productId, req.Urls);

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

public record AddExternalImagesRequest(List<string> Urls);

public class AddExternalImagesRequestValidator : Validator<AddExternalImagesRequest>
{
    public AddExternalImagesRequestValidator()
    {
        RuleFor(x => x.Urls)
            .NotEmpty()
            .Must(urls => urls.All(url => Uri.TryCreate(url, UriKind.Absolute, out _)))
            .WithMessage("Содержатся некорректные URL-адреса");
    }
}