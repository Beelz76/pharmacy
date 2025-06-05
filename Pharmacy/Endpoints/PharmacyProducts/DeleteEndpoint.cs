using FastEndpoints;
using Pharmacy.Services.Interfaces;

namespace Pharmacy.Endpoints.PharmacyProducts;

public class DeleteEndpoint : EndpointWithoutRequest
{
    private readonly IPharmacyProductService  _service;
    
    public DeleteEndpoint(IPharmacyProductService service)
    {
        _service = service;
    }
    
    public override void Configure()
    {
        Delete("pharmacy/{pharmacyId:int}/products/{productId:int}");
        Roles("Admin");
        Tags("Pharmacy");
        Summary(s => { s.Summary = "Удалить товар из аптеки"; });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        int pharmacyId = Route<int>("pharmacyId");
        int productId = Route<int>("productId");
        
        var result = await _service.DeleteAsync(pharmacyId, productId);
        if (result.IsSuccess)
        {
            await SendOkAsync(ct);
        }
        else
        {
            await SendAsync(result.Error, (int)result.Error.Code, ct);
        }
    }
}