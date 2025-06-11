using FastEndpoints;
using FluentValidation;
using Pharmacy.Services.Interfaces;

namespace Pharmacy.Endpoints.PharmacyProducts;

public class AddEndpoint : Endpoint<AddPharmacyProductRequest>
{
    private readonly IPharmacyProductService  _service;

    public AddEndpoint(IPharmacyProductService service)
    {
        _service = service;
    }
        
    public override void Configure()
    {
        Post("pharmacy/{pharmacyId:int}/products");
        Roles("Admin", "Employee");
        Tags("Pharmacy");
        Summary(s => { s.Summary = "Добавить товар в аптеку"; });
    }

    public override async Task HandleAsync(AddPharmacyProductRequest request, CancellationToken ct)
    {
        var pharmacyId = Route<int>("pharmacyId");
        
        var result = await _service.AddAsync(pharmacyId, request);

        if (result.IsSuccess)
        {
            await SendOkAsync(result.Value, ct);
        }
        else
        {
            await SendAsync(result.Error, (int)result.Error.StatusCode, ct);
        }
    }
}

public record AddPharmacyProductRequest(
    int ProductId,
    int StockQuantity,
    decimal? Price,
    bool IsAvailable
);

public class AddPharmacyProductRequestValidator : Validator<AddPharmacyProductRequest>
{
    public AddPharmacyProductRequestValidator()
    {
        RuleFor(x => x.ProductId).GreaterThan(0);
        RuleFor(x => x.StockQuantity).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0).When(x => x.Price.HasValue);
    }
}