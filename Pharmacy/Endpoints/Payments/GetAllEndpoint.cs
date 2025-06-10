using FastEndpoints;
using Pharmacy.Services.Interfaces;
using Pharmacy.Shared.Dto.Payment;

namespace Pharmacy.Endpoints.Payments;

public class GetAllEndpoint : Endpoint<PaymentFilters>
{
    private readonly IPaymentService _service;
    public GetAllEndpoint(IPaymentService service)
    {
        _service = service;
    }

    public override void Configure()
    {
        Post("payments/paginated");
        Roles("Admin");
        Tags("Payments");
    }

    public override async Task HandleAsync(PaymentFilters filters, CancellationToken ct)
    {
        int pageNumber = Query<int>("pageNumber", isRequired: false) == 0 ? 1 : Query<int>("pageNumber", isRequired: false);
        int pageSize = Query<int>("pageSize", isRequired: false) == 0 ? 20 : Query<int>("pageSize", isRequired: false);

        var result = await _service.GetPaginatedAsync(filters, pageNumber, pageSize);
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