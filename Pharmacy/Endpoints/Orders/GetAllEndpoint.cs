using FastEndpoints;
using Pharmacy.Extensions;
using Pharmacy.Services.Interfaces;
using Pharmacy.Shared.Dto;
using Pharmacy.Shared.Enums;

namespace Pharmacy.Endpoints.Orders;

public class GetAllEndpoint : Endpoint<OrderFilters>
{
    private readonly ILogger<GetAllEndpoint> _logger;
    private readonly IOrderService _orderService;
    public GetAllEndpoint(ILogger<GetAllEndpoint> logger, IOrderService orderService)
    {
        _logger = logger;
        _orderService = orderService;
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
        if (userRole != UserRoleEnum.User)
        {
            userId = null;
        }
        
        int pageNumber = Query<int>("pageNumber", isRequired: false) == 0 ? 1 : Query<int>("pageNumber", isRequired: false);
        int pageSize = Query<int>("pageSize", isRequired: false) == 0 ? 20 : Query<int>("pageSize", isRequired: false);
        
        var result = await _orderService.GetPaginatedAsync(filters, pageNumber, pageSize, userId);
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