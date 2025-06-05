using FastEndpoints;
using Pharmacy.Services.Interfaces;

namespace Pharmacy.Endpoints.PharmacyProducts;

public class GetAllEndpoint : EndpointWithoutRequest
{
    private readonly IPharmacyProductService  _service;

    public GetAllEndpoint(IPharmacyProductService service)
    {
        _service = service;
    }
        
    public override void Configure()
    {
        Get("pharmacy/{pharmacyId:int}/products");
        Roles("Admin", "Employee");
        Tags("Pharmacy");
        Summary(s => { s.Summary = "Получить товары в аптеке"; });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var pharmacyId = Route<int>("pharmacyId");
        
        var result = await _service.GetByPharmacyAsync(pharmacyId);

        if (result.IsSuccess)
        {
            await SendOkAsync(result.Value, ct);
        }
        else
        {
            await SendAsync(result.Error, (int)result.Error.Code, cancellation: ct);
        }
    }
}