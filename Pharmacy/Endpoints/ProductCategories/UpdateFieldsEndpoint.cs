using FastEndpoints;
using FluentValidation;
using Pharmacy.Services.Interfaces;
using Pharmacy.Shared.Dto;
using Pharmacy.Shared.Dto.Category;

namespace Pharmacy.Endpoints.ProductCategories;

public class UpdateFieldsEndpoint : Endpoint<UpdateFieldsRequest>
{
    private readonly ILogger<UpdateFieldsEndpoint> _logger;
    private readonly IProductCategoryService _productCategoryService;
    public UpdateFieldsEndpoint(ILogger<UpdateFieldsEndpoint> logger, IProductCategoryService productCategoryService)
    {
        _logger = logger;
        _productCategoryService = productCategoryService;
    }

    public override void Configure()
    {
        Put("categories/{categoryId:int}/fields");
        //Roles("Admin");
        Tags("ProductCategories");
        Summary(s => { s.Summary = "Обновить поля категории"; }); 
    }

    public override async Task HandleAsync(UpdateFieldsRequest request, CancellationToken ct)
    {
        var categoryId = Route<int>("categoryId");
        
        var result = await _productCategoryService.UpdateFieldsAsync(categoryId, request.Fields);
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

public record UpdateFieldsRequest(List<CategoryFieldDto> Fields);

public class UpdateFieldsRequestValidator : Validator<UpdateFieldsRequest>
{
    public UpdateFieldsRequestValidator()
    {
        RuleFor(x => x.Fields)
            .NotEmpty(); 
    }
}