using FastEndpoints;
using FluentValidation;
using Pharmacy.Extensions;
using Pharmacy.Services.Interfaces;

namespace Pharmacy.Endpoints.Cart;

public class RemoveEndpoint : EndpointWithoutRequest
{
    private readonly ILogger<RemoveEndpoint> _logger;
    private readonly ICartService _cartService;
    public RemoveEndpoint(ILogger<RemoveEndpoint> logger, ICartService cartService)
    {
        _logger = logger;
        _cartService = cartService;
    }

    public override void Configure()
    {
        Put("cart/{productId:int}");
        Roles("User");
        Tags("Cart");
        Summary(s => { s.Summary = "Уменьшить количество товара в корзине"; });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var userId = User.GetUserId();
        var productId = Route<int>("productId");
        
        var result = await _cartService.RemoveFromCartAsync(userId, productId);
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