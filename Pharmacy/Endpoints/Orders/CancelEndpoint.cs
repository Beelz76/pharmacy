using FastEndpoints;
using Pharmacy.Extensions;
using Pharmacy.Services.Interfaces;

namespace Pharmacy.Endpoints.Orders;

public class CancelEndpoint : EndpointWithoutRequest
{
    private readonly ILogger<CancelEndpoint> _logger;
    private readonly IOrderService _orderService;
    public CancelEndpoint(ILogger<CancelEndpoint> logger, IOrderService orderService)
    {
        _logger = logger;
        _orderService = orderService;
    }

    public override void Configure()
    {
        Put("orders/{orderId:int}/cancel");
        Roles("User", "Employee", "Admin");
        Tags("Orders");
        Summary(s => { s.Summary = "Отменить заказ"; });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var userId = User.GetUserId();
        if (userId == null)
        {
            await SendUnauthorizedAsync(ct);
            return;
        }
        var orderId = Route<int>("orderId");
        
        var result = await _orderService.CancelAsync(userId.Value, orderId);
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