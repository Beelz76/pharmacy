using FastEndpoints;
using Pharmacy.Extensions;
using Pharmacy.Services.Interfaces;
using Pharmacy.Shared.Dto.Order;
using Pharmacy.Shared.Enums;
using Pharmacy.Shared.Result;

namespace Pharmacy.Endpoints.Orders;

public class GetAllEndpoint : Endpoint<OrderFilters>
{
    private readonly ILogger<GetAllEndpoint> _logger;
    private readonly IOrderService _orderService;
    private readonly IUserService _userService;
    public GetAllEndpoint(ILogger<GetAllEndpoint> logger, IOrderService orderService, IUserService userService)
    {
        _logger = logger;
        _orderService = orderService;
        _userService = userService;
    }

    public override void Configure()
    {
        Post("orders/paginated");
        Roles("Admin", "User", "Employee");
        Tags("Orders");
        Summary(s => { s.Summary = "Получить заказы"; });
    }

    public override async Task HandleAsync(OrderFilters filters, CancellationToken ct)
    {
        var userId = User.GetUserId();
        if (userId == null)
        {
            await SendUnauthorizedAsync(ct);
            return;
        }
        
        var userRole = User.GetUserRole();
        int? pharmacyId = null;
        if (userRole != UserRoleEnum.User)
        {
            userId = null;
            if (userRole == UserRoleEnum.Employee)
            {
                var user = await _userService.GetByIdAsync(User.GetUserId()!.Value);
                if (user.IsFailure || user.Value.Pharmacy == null)
                {
                    await SendAsync(Error.Forbidden("Нет доступа"), 403, ct);
                    return;
                }
                pharmacyId = user.Value.Pharmacy.Id;
                filters = filters with { PharmacyId = pharmacyId };
            }
        }
        
        int pageNumber = Query<int>("pageNumber", isRequired: false) == 0 ? 1 : Query<int>("pageNumber", isRequired: false);
        int pageSize = Query<int>("pageSize", isRequired: false) == 0 ? 20 : Query<int>("pageSize", isRequired: false);
        string? sortBy = Query<string>("sortBy", isRequired: false);
        string? sortOrder = Query<string>("sortOrder", isRequired: false);
        
        var result = await _orderService.GetPaginatedAsync(filters, pageNumber, pageSize, sortBy, sortOrder, userId);
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