using FastEndpoints;
using FluentValidation;
using Pharmacy.Services.Interfaces;
using Pharmacy.Shared.Dto;

namespace Pharmacy.Endpoints.ProductCategories;

public class CreateEndpoint : Endpoint<CreateCategoryRequest>
{
    private readonly ILogger<CreateEndpoint> _logger;
    private readonly IProductCategoryService _productCategoryService;
    public CreateEndpoint(ILogger<CreateEndpoint> logger, IProductCategoryService productCategoryService)
    {
        _logger = logger;
        _productCategoryService = productCategoryService;
    }

    public override void Configure()
    {
        Post("products/category");
        //Roles("Admin");
        Tags("ProductCategories");
        Summary(s => { s.Summary = "Добавить новую категорию товаров"; }); 
    }

    public override async Task HandleAsync(CreateCategoryRequest request, CancellationToken ct)
    {
        var result = await _productCategoryService.CreateAsync(request.Name, request.Description, request.Fields);
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

public record CreateCategoryRequest(string Name, string Description, List<CategoryFieldDto> Fields);

public class CreateCategoryRequestValidator : Validator<CreateCategoryRequest>
{
    public CreateCategoryRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty();
        
        RuleFor(x => x.Description)
            .NotEmpty();
        
        RuleFor(x => x.Fields)
            .NotEmpty(); 
    }
}