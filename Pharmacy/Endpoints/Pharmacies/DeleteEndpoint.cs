using FastEndpoints;
using Pharmacy.Services.Interfaces;

namespace Pharmacy.Endpoints.Pharmacies;

public class DeleteEndpoint : EndpointWithoutRequest
{
    private readonly IPharmacyService _service;

    public DeleteEndpoint(IPharmacyService service)
    {
        _service = service;
    }

    public override void Configure()
    {
        Delete("pharmacies/{id:int}");
        Roles("Admin");
        Tags("Pharmacy");
        Summary(s => { s.Summary = "Удалить аптеку"; });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        int id = Route<int>("id");
        var result = await _service.DeleteAsync(id);
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