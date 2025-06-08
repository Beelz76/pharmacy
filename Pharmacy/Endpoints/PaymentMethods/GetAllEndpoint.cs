using FastEndpoints;
using Pharmacy.Services.Interfaces;

namespace Pharmacy.Endpoints.PaymentMethods;

public class GetAllEndpoint : EndpointWithoutRequest
{
    private readonly IPaymentMethodService _service;

    public GetAllEndpoint(IPaymentMethodService service)
    {
        _service = service;
    }

    public override void Configure()
    {
        Get("payment-methods");
        Roles("Admin");
        Tags("Payments");
        Summary(s => s.Summary = "Получить все методы оплаты");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await _service.GetAllAsync();
        await SendOkAsync(result, ct);
    }
}