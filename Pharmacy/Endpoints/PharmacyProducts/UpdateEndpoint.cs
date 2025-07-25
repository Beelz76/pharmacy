﻿using FastEndpoints;
using FluentValidation;
using Pharmacy.Services.Interfaces;

namespace Pharmacy.Endpoints.PharmacyProducts;

public class UpdateEndpoint : Endpoint<UpdatePharmacyProductRequest>
{
    private readonly IPharmacyProductService  _service;

    public UpdateEndpoint(IPharmacyProductService service)
    {
        _service = service;
    }
        
    public override void Configure()
    {
        Put("pharmacy/{pharmacyId:int}/products/{productId:int}");
        Roles("Admin", "Employee");
        Tags("Pharmacy");
        Summary(s => { s.Summary = "Обновить товар в аптеке"; });
    }

    public override async Task HandleAsync(UpdatePharmacyProductRequest request, CancellationToken ct)
    {
        var pharmacyId = Route<int>("pharmacyId");
        var productId = Route<int>("productId");
        
        var result = await _service.UpdateAsync(pharmacyId, productId, request);

        if (result.IsSuccess)
        {
            await SendOkAsync(ct);
        }
        else
        {
            await SendAsync(result.Error, (int)result.Error.StatusCode, ct);
        }
    }
}

public record UpdatePharmacyProductRequest(int StockQuantity, decimal? Price, bool IsAvailable);

public class UpdatePharmacyProductRequestValidator : Validator<UpdatePharmacyProductRequest>
{
    public UpdatePharmacyProductRequestValidator()
    {
        RuleFor(x => x.StockQuantity).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0).When(x => x.Price.HasValue);
    }
}