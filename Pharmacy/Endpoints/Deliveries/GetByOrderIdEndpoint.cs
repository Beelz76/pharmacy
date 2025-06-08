using FastEndpoints;
using Pharmacy.Services.Interfaces;

namespace Pharmacy.Endpoints.Deliveries;

public class GetByOrderIdEndpoint : EndpointWithoutRequest
{
    private readonly IDeliveryService _service;
    public GetByOrderIdEndpoint(IDeliveryService service)
    {
        _service = service;
    }

    public override void Configure()
    {
        Get("deliveries/order/{orderId:int}");
        Roles("Admin");
        Tags("Deliveries");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var orderId = Route<int>("orderId");
        var result = await _service.GetByOrderIdAsync(orderId);
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