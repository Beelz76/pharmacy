using FastEndpoints;
using FluentValidation;
using Pharmacy.Services.Interfaces;
using Pharmacy.Shared.Dto;

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
        Put("products/{productId:int}");
        Roles("Admin");
        Tags("Products");
        Summary(s => { s.Summary = "Изменить товар"; }); 
    }

    public override async Task HandleAsync(UpdateProductRequest request, CancellationToken ct)
    {
        int productId = Route<int>("productId");
        
        var result = await _productService.UpdateAsync(productId, request);
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

public record UpdateProductRequest(
    string Name,
    decimal Price,
    int StockQuantity,
    int CategoryId,
    int ManufacturerId,
    string Description,
    DateTime? ExpirationDate,
    bool IsAvailable,
    bool IsPrescriptionRequired,
    List<ProductPropertyDto> Properties);
    
public class UpdateProductRequestValidator : Validator<UpdateProductRequest>
{
    public UpdateProductRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty();

        RuleFor(x => x.Price)
            .NotEmpty();
        
        RuleFor(x => x.StockQuantity)
            .NotEmpty();
        
        RuleFor(x => x.CategoryId)
            .NotEmpty();

        RuleFor(x => x.ManufacturerId)
            .NotEmpty();
        
        RuleFor(x => x.Description)
            .NotEmpty();
    }
}