using FastEndpoints;
using FluentValidation;
using Pharmacy.Extensions;
using Pharmacy.Services.Interfaces;

namespace Pharmacy.Endpoints.Cart;

public class SetQuantityEndpoint : Endpoint<SetCartQuantityRequest>
{
    private readonly ILogger<SetQuantityEndpoint> _logger;
    private readonly ICartService _cartService;
    public SetQuantityEndpoint(ILogger<SetQuantityEndpoint> logger, ICartService cartService)
    {
        _logger = logger;
        _cartService = cartService;
    }

    public override void Configure()
    {
        Put("cart/set-quantity");
        Roles("User");
        Tags("Cart");
        Summary(s => { s.Summary = "Установить конкретное количество товара в корзине"; });
    }

    public override async Task HandleAsync(SetCartQuantityRequest request, CancellationToken ct)
    {
        var userId = User.GetUserId();
        
        var result = await _cartService.SetQuantityAsync(userId, request.ProductId, request.Quantity);
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

public record SetCartQuantityRequest(int ProductId, int Quantity);

public class SetCartQuantityRequestValidator : Validator<SetCartQuantityRequest>
{
    public SetCartQuantityRequestValidator()
    {
        RuleFor(x => x.Quantity)
            .NotEmpty()
            .GreaterThanOrEqualTo(1); 
    }
}