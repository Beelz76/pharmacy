using FastEndpoints;
using Pharmacy.Services.Interfaces;

namespace Pharmacy.Endpoints.Pharmacies;

public class GetAllEndpoint : EndpointWithoutRequest
{
    private readonly IPharmacyService _pharmacyService;
    public GetAllEndpoint(IPharmacyService pharmacyService)
    {
        _pharmacyService = pharmacyService;
    }

    public override void Configure()
    {
        Get("pharmacies");
        Roles("Admin");
        Tags("Pharmacy");
        Summary(s => { s.Summary = "Получить все аптеки"; });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await _pharmacyService.GetAllAsync();
        if (result.IsSuccess)
        {
            await SendOkAsync(result.Value, ct);
        }
        else
        {
            await SendAsync(result.Error, (int)result.Error.Code, ct);
        }
    }
}