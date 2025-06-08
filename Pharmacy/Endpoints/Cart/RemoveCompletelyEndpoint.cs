using FastEndpoints;
using FluentValidation;
using Pharmacy.Extensions;
using Pharmacy.Services.Interfaces;

namespace Pharmacy.Endpoints.Cart;

public class RemoveCompletelyEndpoint : EndpointWithoutRequest
{
    private readonly ILogger<RemoveCompletelyEndpoint> _logger;
    private readonly ICartService _cartService;
    public RemoveCompletelyEndpoint(ILogger<RemoveCompletelyEndpoint> logger, ICartService cartService)
    {
        _logger = logger;
        _cartService = cartService;
    }

    public override void Configure()
    {
        Delete("cart/{productId:int}");
        Roles("User");
        Tags("Cart");
        Summary(s => { s.Summary = "Удалить товар из корзины"; });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var userId = User.GetUserId();
        if (userId == null)
        {
            await SendUnauthorizedAsync(ct);
            return;
        }
        var productId = Route<int>("productId");
        
        var result = await _cartService.RemoveFromCartCompletelyAsync(userId.Value, productId);
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