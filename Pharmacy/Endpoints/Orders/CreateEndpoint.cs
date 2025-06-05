using FastEndpoints;
using FluentValidation;
using Pharmacy.Extensions;
using Pharmacy.Services.Interfaces;
using Pharmacy.Shared.Enums;

namespace Pharmacy.Endpoints.Orders;

public class CreateEndpoint : Endpoint<CreateOrderRequest>
{
    private readonly ILogger<CreateEndpoint> _logger;
    private readonly IOrderService _orderService;
    public CreateEndpoint(ILogger<CreateEndpoint> logger, IOrderService orderService)
    {
        _logger = logger;
        _orderService = orderService;
    }

    public override void Configure()
    {
        Post("orders");
        Roles("User");
        Tags("Orders");
        Summary(s => { s.Summary = "Сформировать заказ"; });
    }

    public override async Task HandleAsync(CreateOrderRequest request, CancellationToken ct)
    {
        var userId = User.GetUserId();
        if (userId == null)
        {
            await SendUnauthorizedAsync(ct);
            return;
        }
        
        var result = await _orderService.CreateAsync(userId.Value, request.PharmacyAddress, request.PaymentMethod);
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

public record CreateOrderRequest(string PharmacyAddress, PaymentMethodEnum PaymentMethod);

public class CreateOrderRequestValidator : Validator<CreateOrderRequest>
{
    public CreateOrderRequestValidator()
    {
        RuleFor(x => x.PharmacyAddress)
            .NotEmpty();
        
        RuleFor(x => x.PaymentMethod)
            .NotEmpty();
    }
}