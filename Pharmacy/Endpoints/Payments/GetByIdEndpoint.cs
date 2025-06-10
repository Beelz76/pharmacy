using FastEndpoints;
using Pharmacy.Services.Interfaces;

namespace Pharmacy.Endpoints.Payments;

public class GetByIdEndpoint : EndpointWithoutRequest
{
    private readonly IPaymentService _service;
    public GetByIdEndpoint(IPaymentService service)
    {
        _service = service;
    }

    public override void Configure()
    {
        Get("payments/{id:int}");
        Roles("Admin");
        Tags("Payments");
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