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
        Summary(s => { s.Summary = "Получить аптеки"; });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        int pageNumber = Query<int>("pageNumber", isRequired: false) == 0 ? 1 : Query<int>("pageNumber", isRequired: false);
        int pageSize = Query<int>("pageSize", isRequired: false) == 0 ? 15 : Query<int>("pageSize", isRequired: false);
        string? search = Query<string>("search", isRequired: false);

        var result = await _pharmacyService.GetPaginatedAsync(search, pageNumber, pageSize);
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