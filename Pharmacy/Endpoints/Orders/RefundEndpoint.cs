using FastEndpoints;
using Pharmacy.Extensions;
using Pharmacy.Services.Interfaces;
using Pharmacy.Shared.Enums;
using Pharmacy.Shared.Result;

namespace Pharmacy.Endpoints.Orders;

public class RefundEndpoint : EndpointWithoutRequest
{
    private readonly ILogger<RefundEndpoint> _logger;
    private readonly IOrderService _orderService;
    private readonly IUserService _userService;
    public RefundEndpoint(ILogger<RefundEndpoint> logger, IOrderService orderService, IUserService userService)
    {
        _logger = logger;
        _orderService = orderService;
        _userService = userService;
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
        
        var role = User.GetUserRole();
        int? pharmacyId = null;

        if (role == UserRoleEnum.Employee)
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

        var result = await _orderService.RefundAsync(orderId, pharmacyId);
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