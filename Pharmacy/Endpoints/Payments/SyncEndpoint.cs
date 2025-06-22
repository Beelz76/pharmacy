using FastEndpoints;
using Pharmacy.Services.Interfaces;
using Pharmacy.Shared.Enums;

namespace Pharmacy.Endpoints.Payments;

public class SyncEndpoint : EndpointWithoutRequest
{
    private readonly IPaymentService _service;

    public SyncEndpoint(IPaymentService service)
    {
        _service = service;
    }

    public override void Configure()
    {
        Post("payments/{id:int}/sync");
        Roles("Admin");
        Tags("Payments");
        Summary(s => s.Summary = "Сверить статус платежа с YooKassa");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<int>("id");
        var result = await _service.SyncStatusWithYooKassaAsync(id);
        if (result.IsSuccess)
        {
            await SendOkAsync(new { status = result.Value.ToString() }, ct);
        }
        else
        {
            await SendAsync(result.Error, (int)result.Error.StatusCode, ct);
        }
    }
}