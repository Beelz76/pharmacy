using FastEndpoints;
using FluentValidation;
using Pharmacy.Services.Interfaces;
using Pharmacy.Shared.Dto;

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
        Roles("Admin");
        Tags("Products");
        Summary(s => { s.Summary = "Добавить новый товар"; }); 
    }

    public override async Task HandleAsync(CreateProductRequest request, CancellationToken ct)
    {
        var result = await _productService.CreateProductAsync(request);
        if (result.IsSuccess)
        {
            await SendOkAsync (result.Value, ct);
        }
        else
        {
            await SendAsync(result.Error, (int)result.Error.Code, ct);
        }
    }
}

public record CreateProductRequest(
    string Name,
    decimal Price,
    int StockQuantity,
    int CategoryId,
    int ManufacturerId,
    string Description,
    string ExtendedDescription,
    DateTime? ExpirationDate,
    bool IsAvailable,
    bool IsPrescriptionRequired,
    List<ProductPropertyDto> Properties);
    
public class CreateProductRequestValidator : Validator<CreateProductRequest>
{
    public CreateProductRequestValidator()
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