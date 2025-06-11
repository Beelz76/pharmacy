using FastEndpoints;
using Pharmacy.Extensions;
using Pharmacy.Services.Interfaces;
using Pharmacy.Shared.Enums;
using Pharmacy.Shared.Result;

namespace Pharmacy.Endpoints.Orders;

public class GetByIdEndpoint : EndpointWithoutRequest
{
    private readonly ILogger<GetByIdEndpoint> _logger;
    private readonly IOrderService _orderService;
    private readonly IUserService _userService;
    public GetByIdEndpoint(ILogger<GetByIdEndpoint> logger, IOrderService orderService, IUserService userService)
    {
        _logger = logger;
        _orderService = orderService;
        _userService = userService;
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
        
        int? pharmacyId = null;
        if (userRole == UserRoleEnum.Employee)
        {
            var user = await _userService.GetByIdAsync(userId.Value);
            if (user.IsFailure || user.Value.Pharmacy == null)
            {
                await SendAsync(Error.Forbidden("Нет доступа"), 403, ct);
                return;
            }
            pharmacyId = user.Value.Pharmacy.Id;
        }

        var result = await _orderService.GetByIdAsync(orderId, userId.Value, userRole, pharmacyId);
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