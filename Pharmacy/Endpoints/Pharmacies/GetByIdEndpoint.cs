using FastEndpoints;
using Pharmacy.Services.Interfaces;

namespace Pharmacy.Endpoints.Pharmacies;

public class GetByIdEndpoint : EndpointWithoutRequest
{
    private readonly IPharmacyService _service;

    public GetByIdEndpoint(IPharmacyService service)
    {
        _service = service;
    }

    public override void Configure()
    {
        Get("pharmacies/{id:int}");
        Roles("Admin", "Employee");
        Tags("Pharmacy");
        Summary(s => { s.Summary = "Получить аптеку по id"; });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<int>("id");
        var result = await _service.GetByIdAsync(id);
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