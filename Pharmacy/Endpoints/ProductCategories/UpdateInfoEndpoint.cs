using FastEndpoints;
using FluentValidation;
using Pharmacy.Services.Interfaces;
using Pharmacy.Shared.Dto;

namespace Pharmacy.Endpoints.ProductCategories;

public class UpdateInfoEndpoint : Endpoint<UpdateCategoryRequest>
{
    private readonly ILogger<UpdateInfoEndpoint> _logger;
    private readonly IProductCategoryService _productCategoryService;
    public UpdateInfoEndpoint(ILogger<UpdateInfoEndpoint> logger, IProductCategoryService productCategoryService)
    {
        _logger = logger;
        _productCategoryService = productCategoryService;
    }

    public override void Configure()
    {
        Put("categories/{categoryId:int}");
        //Roles("Admin");
        Tags("ProductCategories");
        Summary(s => { s.Summary = "Обновление названия/описания категории"; }); 
    }

    public override async Task HandleAsync(UpdateCategoryRequest request, CancellationToken ct)
    {
        int categoryId = Route<int>("categoryId");
        
        var result = await _productCategoryService.UpdateBasicInfoAsync(categoryId, request.Name, request.Description, request.ParentCategoryId);
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

public record UpdateCategoryRequest(string Name, string Description, int? ParentCategoryId);

public class UpdateCategoryRequestValidator : Validator<UpdateCategoryRequest>
{
    public UpdateCategoryRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty();
        
        RuleFor(x => x.Description)
            .NotEmpty();
        
        // When(x => x.Fields is not null, () =>
        // {
        //     RuleFor(x => x.Fields)
        //         .Must(HaveUniqueKeys)
        //         .WithMessage("Ключи полей должны быть уникальными.");
        //
        //     RuleForEach(x => x.Fields!).ChildRules(fields =>
        //     {
        //         fields.RuleFor(f => f.Key)
        //             .NotEmpty().WithMessage("Ключ поля не должен быть пустым.");
        //
        //         fields.RuleFor(f => f.Label)
        //             .NotEmpty().WithMessage(f => $"Метка поля для \"{f.Key}\" не должна быть пустой.");
        //
        //         // fields.RuleFor(f => f.Type)
        //         //     .NotEmpty().WithMessage(f => $"Тип поля для \"{f.Key}\" не должен быть пустым.")
        //         //     .Must(type => SupportedTypes.Contains(type.ToLower()))
        //         //     .WithMessage(f => $"Тип поля \"{f.Type}\" для ключа \"{f.Key}\" не поддерживается.");
        //     });
        // });
    }
    
    private bool HaveUniqueKeys(List<CategoryFieldDto> fields)
    {
        var uniqueKeys = fields.Select(f => f.Key).Distinct().Count();
        return uniqueKeys == fields.Count;
    }
}