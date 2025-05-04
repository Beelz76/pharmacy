using FastEndpoints;
using Pharmacy.Extensions;
using Pharmacy.Services.Interfaces;

namespace Pharmacy.Endpoints.Orders;

public class RefundEndpoint : EndpointWithoutRequest
{
    private readonly ILogger<RefundEndpoint> _logger;
    private readonly IOrderService _orderService;
    public RefundEndpoint(ILogger<RefundEndpoint> logger, IOrderService orderService)
    {
        _logger = logger;
        _orderService = orderService;
    }

    public override void Configure()
    {
        Put("orders/{orderId:int}/refund");
        Roles("User", "Admin", "Employee");
        Tags("Orders");
        Summary(s => { s.Summary = "Возврат заказа"; });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var orderId = Route<int>("orderId");
        
        var result = await _orderService.RefundAsync(orderId);
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