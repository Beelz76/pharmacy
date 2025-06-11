using FastEndpoints;
using FluentValidation;
using Pharmacy.Extensions;
using Pharmacy.Services.Interfaces;
using Pharmacy.Shared.Enums;
using Pharmacy.Shared.Result;

namespace Pharmacy.Endpoints.Orders;

public class CancelEndpoint : Endpoint<CancelOrderRequest>
{
    private readonly ILogger<CancelEndpoint> _logger;
    private readonly IOrderService _orderService;
    private readonly IUserService _userService;
    public CancelEndpoint(ILogger<CancelEndpoint> logger, IOrderService orderService, IUserService userService)
    {
        _logger = logger;
        _orderService = orderService;
        _userService = userService;
    }

    public override void Configure()
    {
        Put("orders/{orderId:int}/cancel");
        Roles("User", "Employee", "Admin");
        Tags("Orders");
        Summary(s => { s.Summary = "Отменить заказ"; });
    }

    public override async Task HandleAsync(CancelOrderRequest request, CancellationToken ct)
    {
        var userId = User.GetUserId();
        if (userId == null)
        {
            await SendUnauthorizedAsync(ct);
            return;
        }
        var orderId = Route<int>("orderId");
        
        var role = User.GetUserRole();

        Result result;
        if (role == UserRoleEnum.User)
        {
            result = await _orderService.CancelAsync(userId.Value, orderId, request.Comment);
        }
        else
        {
            int? pharmacyId = null;
            if (role == UserRoleEnum.Employee)
            {
                var user = await _userService.GetByIdAsync(userId.Value);
                if (user.IsFailure || user.Value.Pharmacy == null)
                {
                    await SendAsync(Error.Forbidden("Нет доступа"), 403, ct);
                    return;
                }
                pharmacyId = user.Value.Pharmacy.Id;
            }

            result = await _orderService.CancelByStaffAsync(orderId, request.Comment, pharmacyId);
        }
        
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

public record CancelOrderRequest(string? Comment);

public class CancelOrderRequestValidator : Validator<CancelOrderRequest>
{
    public CancelOrderRequestValidator()
    {
        RuleFor(x => x.Comment).MaximumLength(500);
    }
}