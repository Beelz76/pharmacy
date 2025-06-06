using FastEndpoints;
using Pharmacy.Services.Interfaces;

namespace Pharmacy.Endpoints.Orders;

public class MarkAsDeliveredEndpoint : EndpointWithoutRequest
{
    private readonly ILogger<MarkAsDeliveredEndpoint> _logger;
    private readonly IOrderService _orderService;
    public MarkAsDeliveredEndpoint(ILogger<MarkAsDeliveredEndpoint> logger, IOrderService orderService)
    {
        _logger = logger;
        _orderService = orderService;
    }

    public override void Configure()
    {
        Post("orders/{orderId:int}/set-delivered");
        Roles("Admin", "Employee");
        Tags("Orders");
        Summary(s => { s.Summary = "Проставить статус \"Доставлен\""; });;
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var orderId = Route<int>("orderId");
        
        var result = await _orderService.MarkAsDeliveredAsync(orderId);
        if (result.IsSuccess)
        {
            await SendOkAsync(ct);
        }
        else
        {
            await SendAsync(result.Error, (int)result.Error.Code, ct);
        }
    }
}