using FastEndpoints;
using Pharmacy.Services.Interfaces;

namespace Pharmacy.Endpoints.Orders;

public class GetStatusesEndpoint : EndpointWithoutRequest
{
    private readonly ILogger<GetStatusesEndpoint> _logger;
    private readonly IOrderService _orderService;
    public GetStatusesEndpoint(ILogger<GetStatusesEndpoint> logger, IOrderService orderService)
    {
        _logger = logger;
        _orderService = orderService;
    }

    public override void Configure()
    {
        Get("orders/statuses");
        //Roles("Admin", "Employee");
        Tags("Orders");
        Summary(s => { s.Summary = "Получить возможные статусы заказа"; });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await _orderService.GetAllStatusesAsync();
        await SendOkAsync(result, ct);
    }
}