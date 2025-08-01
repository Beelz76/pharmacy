﻿using FastEndpoints;
using FluentValidation;
using Pharmacy.Services.Interfaces;
using Pharmacy.Shared.Dto.Category;

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
        Post("categories");
        Roles("Admin");
        Tags("ProductCategories");
        Summary(s => { s.Summary = "Добавить новую категорию товаров"; }); 
    }

    public override async Task HandleAsync(CreateCategoryRequest request, CancellationToken ct)
    {
        var result = await _productCategoryService.CreateAsync(request.Name, request.Description, request.ParentCategoryId, request.Fields);
        if (result.IsSuccess)
        {
            await SendOkAsync (result.Value, ct);
        }
        else
        {
            await SendAsync(result.Error, (int)result.Error.StatusCode, ct);
        }
    }
}

public record CreateCategoryRequest(string Name, string Description, int? ParentCategoryId, List<CategoryFieldDto> Fields);

public class CreateCategoryRequestValidator : Validator<CreateCategoryRequest>
{
    public CreateCategoryRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty();
        
        RuleFor(x => x.Description)
            .NotEmpty();
        
        RuleFor(x => x.Fields)
            .NotEmpty()
            .When(x => x.ParentCategoryId == 0 || x.ParentCategoryId is null); 
    }
}