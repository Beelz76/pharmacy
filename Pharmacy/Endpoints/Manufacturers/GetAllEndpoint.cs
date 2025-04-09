using FastEndpoints;
using Pharmacy.Services.Interfaces;

namespace Pharmacy.Endpoints.Manufacturers;

public class GetAllEndpoint : EndpointWithoutRequest
{
    private readonly IManufacturerService _manufacturerService;

    public GetAllEndpoint(IManufacturerService manufacturerService)
    {
        _manufacturerService = manufacturerService;
    }
        
    public override void Configure()
    {
        Get("manufacturers");
        AllowAnonymous();
        Tags("Manufacturer");
        Summary(s => { s.Summary = "Получить всех производителей"; });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await _manufacturerService.GetAllAsync();

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