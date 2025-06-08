using FastEndpoints;
using FluentValidation;
using Pharmacy.Services.Interfaces;
using Pharmacy.Shared.Dto;
using Pharmacy.Shared.Dto.Category;

namespace Pharmacy.Endpoints.ProductCategories;

public class AddFieldsEndpoint : Endpoint<AddFieldsRequest>
{
    private readonly ILogger<AddFieldsEndpoint> _logger;
    private readonly IProductCategoryService _productCategoryService;
    public AddFieldsEndpoint(ILogger<AddFieldsEndpoint> logger, IProductCategoryService productCategoryService)
    {
        _logger = logger;
        _productCategoryService = productCategoryService;
    }

    public override void Configure()
    {
        Post("categories/{categoryId:int}/fields");
        //Roles("Admin");
        Tags("ProductCategories");
        Summary(s => { s.Summary = "Добавить поля к категории"; }); 
    }

    public override async Task HandleAsync(AddFieldsRequest request, CancellationToken ct)
    {
        var categoryId = Route<int>("categoryId");
        
        var result = await _productCategoryService.AddFieldsAsync(categoryId, request.Fields);
        if (result.IsSuccess)
        {
            await SendOkAsync (ct);
        }
        else
        {
            await SendAsync(result.Error, (int)result.Error.StatusCode, ct);
        }
    }
}

public record AddFieldsRequest(List<CategoryFieldDto> Fields);

public class AddFieldsRequestValidator : Validator<AddFieldsRequest>
{
    public AddFieldsRequestValidator()
    {
        RuleFor(x => x.Fields)
            .NotEmpty(); 
    }
}