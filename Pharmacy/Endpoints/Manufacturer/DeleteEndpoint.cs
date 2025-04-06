using FastEndpoints;
using Pharmacy.Services.Interfaces;

namespace Pharmacy.Endpoints.Manufacturer;

public class DeleteEndpoint : EndpointWithoutRequest
{
    private readonly IManufacturerService _manufacturerService;

    public DeleteEndpoint(IManufacturerService manufacturerService)
    {
        _manufacturerService = manufacturerService;
    }
        
    public override void Configure()
    {
        Delete("manufacturers/{id:int}");
        AllowAnonymous();
        Tags("Manufacturer");
        Summary(s => { s.Summary = "Удалить производителя"; });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        int id = Route<int>("id");
        
        var result = await _manufacturerService.DeleteAsync(id);
        if (!result.IsSuccess)
        {
            await SendAsync(result.Error, (int)result.Error.Code, cancellation: ct);
        }
        else
        {
            await SendOkAsync(ct);
        }
    }
}