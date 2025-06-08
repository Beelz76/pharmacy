using FastEndpoints;
using Pharmacy.Services.Interfaces;

namespace Pharmacy.Endpoints.Manufacturers;

public class GetByIdEndpoint : EndpointWithoutRequest
{
    private readonly IManufacturerService _manufacturerService;

    public GetByIdEndpoint(IManufacturerService manufacturerService)
    {
        _manufacturerService = manufacturerService;
    }
        
    public override void Configure()
    {
        Get("manufacturers/{manufacturerId:int}");
        Roles("Admin");
        Tags("Manufacturer");
        Summary(s => { s.Summary = "Получить производителя по id"; });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var manufacturerId = Route<int>("manufacturerId");
        
        var result = await _manufacturerService.GetByIdAsync(manufacturerId);
        if (result.IsSuccess)
        {
            await SendOkAsync(result.Value, ct);
        }
        else
        {
            await SendAsync(result.Error, (int)result.Error.StatusCode, cancellation: ct);
        }
    }
}