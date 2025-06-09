using FastEndpoints;
using Pharmacy.Services.Interfaces;

namespace Pharmacy.Endpoints.Manufacturers;

public class GetCountriesEndpoint : EndpointWithoutRequest
{
    private readonly IManufacturerService _service;
    public GetCountriesEndpoint(IManufacturerService service)
    {
        _service = service;
    }

    public override void Configure()
    {
        Get("manufacturers/countries");
        AllowAnonymous();
        Tags("Manufacturer");
        Summary(s => { s.Summary = "Получить список стран производителей"; });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await _service.GetCountriesAsync();
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