﻿using FastEndpoints;
using FluentValidation;
using Pharmacy.Services.Interfaces;

namespace Pharmacy.Endpoints.ProductCategories;

public class DeleteFieldsEndpoint : Endpoint<DeleteFieldsRequest>
{
    private readonly ILogger<DeleteFieldsEndpoint> _logger;
    private readonly IProductCategoryService _productCategoryService;
    public DeleteFieldsEndpoint(ILogger<DeleteFieldsEndpoint> logger, IProductCategoryService productCategoryService)
    {
        _logger = logger;
        _productCategoryService = productCategoryService;
    }

    public override void Configure()
    {
        Delete("categories/{categoryId:int}/fields");
        //Roles("Admin");
        Tags("ProductCategories");
        Summary(s => { s.Summary = "Удалить поля категории"; }); 
    }

    public override async Task HandleAsync(DeleteFieldsRequest request, CancellationToken ct)
    {
        var categoryId = Route<int>("categoryId");
        
        var result = await _productCategoryService.DeleteFieldsAsync(categoryId, request.FieldIds);
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

public record DeleteFieldsRequest(List<int> FieldIds);

public class DeleteFieldsRequestValidator : Validator<DeleteFieldsRequest>
{
    public DeleteFieldsRequestValidator()
    {
        RuleFor(x => x.FieldIds)
            .NotEmpty(); 
    }
}