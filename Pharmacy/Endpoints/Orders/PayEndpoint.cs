using FastEndpoints;
using Pharmacy.Extensions;
using Pharmacy.Services.Interfaces;

namespace Pharmacy.Endpoints.Orders;

public class PayEndpoint : EndpointWithoutRequest
{
    private readonly ILogger<PayEndpoint> _logger;
    private readonly IOrderService _orderService;
    public PayEndpoint(ILogger<PayEndpoint> logger, IOrderService orderService)
    {
        _logger = logger;
        _orderService = orderService;
    }

    public override void Configure()
    {
        Post("orders/{orderId:int}/pay");
        Roles("User");
        Tags("Orders");
        Summary(s => { s.Summary = "Оплатить заказ"; });
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
        
        var result = await _orderService.PayAsync(orderId, userId.Value);
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