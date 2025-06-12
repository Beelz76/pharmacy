using FastEndpoints;
using Pharmacy.Extensions;
using Pharmacy.Services.Interfaces;
using Pharmacy.Shared.Result;

namespace Pharmacy.Endpoints.Orders;

public class RepeatEndpoint : EndpointWithoutRequest
{
    private readonly ILogger<RepeatEndpoint> _logger;
    private readonly IOrderService _orderService;

    public RepeatEndpoint(ILogger<RepeatEndpoint> logger, IOrderService orderService)
    {
        _logger = logger;
        _orderService = orderService;
    }

    public override void Configure()
    {
        Post("orders/{orderId:int}/repeat");
        Roles("User");
        Tags("Orders");
        Summary(s => { s.Summary = "Повторить заказ"; });
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
        var result = await _orderService.RepeatAsync(orderId, userId.Value);
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