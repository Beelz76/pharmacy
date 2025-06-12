using FastEndpoints;
using Pharmacy.Services.Interfaces;

namespace Pharmacy.Endpoints.PaymentStatuses;

public class GetAllEndpoint : EndpointWithoutRequest
{
    private readonly IPaymentStatusService _service;

    public GetAllEndpoint(IPaymentStatusService service)
    {
        _service = service;
    }

    public override void Configure()
    {
        Get("payments/statuses");
        Roles("Admin", "Employee");
        Tags("Payments");
        Summary(s => s.Summary = "Получить все статусы оплаты");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await _service.GetAllAsync();
        await SendOkAsync(result, ct);
    }
}