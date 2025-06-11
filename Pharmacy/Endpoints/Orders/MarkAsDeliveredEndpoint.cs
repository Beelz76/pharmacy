using FastEndpoints;
using Pharmacy.Extensions;
using Pharmacy.Services.Interfaces;
using Pharmacy.Shared.Enums;
using Pharmacy.Shared.Result;

namespace Pharmacy.Endpoints.Orders;

public class MarkAsDeliveredEndpoint : EndpointWithoutRequest
{
    private readonly ILogger<MarkAsDeliveredEndpoint> _logger;
    private readonly IOrderService _orderService;
    private readonly IUserService _userService;
    public MarkAsDeliveredEndpoint(ILogger<MarkAsDeliveredEndpoint> logger, IOrderService orderService, IUserService userService)
    {
        _logger = logger;
        _orderService = orderService;
        _userService = userService;
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
        
        int? pharmacyId = null;
        if (User.GetUserRole() == UserRoleEnum.Employee)
        {
            var userId = User.GetUserId();
            if (userId == null)
            {
                await SendUnauthorizedAsync(ct);
                return;
            }
            var user = await _userService.GetByIdAsync(userId.Value);
            if (user.IsFailure || user.Value.Pharmacy == null)
            {
                await SendAsync(Error.Forbidden("Нет доступа"), 403, ct);
                return;
            }
            pharmacyId = user.Value.Pharmacy.Id;
        }

        var result = await _orderService.MarkAsDeliveredAsync(orderId, pharmacyId);
        if (result.IsSuccess)
        {
            await SendOkAsync(ct);
        }
        else
        {
            await SendAsync(result.Error, (int)result.Error.StatusCode, ct);
        }
    }
}