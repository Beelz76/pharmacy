using FastEndpoints;
using Pharmacy.Extensions;
using Pharmacy.Services.Interfaces;
using Pharmacy.Shared.Enums;

namespace Pharmacy.Endpoints.Orders;

public class GetByIdEndpoint : EndpointWithoutRequest
{
    private readonly ILogger<GetByIdEndpoint> _logger;
    private readonly IOrderService _orderService;
    public GetByIdEndpoint(ILogger<GetByIdEndpoint> logger, IOrderService orderService)
    {
        _logger = logger;
        _orderService = orderService;
    }

    public override void Configure()
    {
        Get("orders/{orderId:int}");
        Roles("Admin", "User", "Employee");
        Tags("Orders");
        Summary(s => { s.Summary = "Получить заказ по id"; });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var userId = User.GetUserId();
        if (userId == null)
        {
            await SendUnauthorizedAsync(ct);
            return;
        }
        
        var userRole = User.GetUserRole();
        var orderId = Route<int>("orderId");
        
        var result = await _orderService.GetByIdAsync(orderId, userId.Value, userRole);
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