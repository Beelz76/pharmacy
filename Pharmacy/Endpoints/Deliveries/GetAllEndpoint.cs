using FastEndpoints;
using Pharmacy.Services.Interfaces;
using Pharmacy.Shared.Dto;
using Pharmacy.Shared.Dto.Delivery;

namespace Pharmacy.Endpoints.Deliveries;

public class GetAllEndpoint : Endpoint<DeliveryFilters>
{
    private readonly IDeliveryService _service;
    public GetAllEndpoint(IDeliveryService service)
    {
        _service = service;
    }

    public override void Configure()
    {
        Post("deliveries/paginated");
        Roles("Admin");
        Tags("Deliveries");
    }

    public override async Task HandleAsync(DeliveryFilters filters, CancellationToken ct)
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