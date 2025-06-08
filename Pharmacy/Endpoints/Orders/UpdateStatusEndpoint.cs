using FastEndpoints;
using FluentValidation;
using Pharmacy.Services.Interfaces;
using Pharmacy.Shared.Enums;

namespace Pharmacy.Endpoints.Orders;

public class UpdateStatusEndpoint : Endpoint<UpdateOrderStatusRequest>
{
    private readonly ILogger<UpdateStatusEndpoint> _logger;
    private readonly IOrderService _orderService;
    public UpdateStatusEndpoint(ILogger<UpdateStatusEndpoint> logger, IOrderService orderService)
    {
        _logger = logger;
        _orderService = orderService;
    }

    public override void Configure()
    {
        Put("orders/{orderId:int}/status");
        //Roles("Admin", "Employee");
        Tags("Orders");
        Summary(s => { s.Summary = "Обновить статус заказа"; });;
    }

    public override async Task HandleAsync(UpdateOrderStatusRequest request, CancellationToken ct)
    {
        var orderId = Route<int>("orderId");
        
        var result = await _orderService.UpdateStatusAsync(orderId, request.NewStatus);
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

public record UpdateOrderStatusRequest(OrderStatusEnum NewStatus);

public class UpdateOrderStatusRequestValidator : Validator<UpdateOrderStatusRequest>
{
    public UpdateOrderStatusRequestValidator()
    {
        RuleFor(x => x.NewStatus)
            .NotEmpty(); 
    }
}