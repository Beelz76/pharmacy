using FastEndpoints;
using Pharmacy.Extensions;
using Pharmacy.Services.Interfaces;

namespace Pharmacy.Endpoints.Cart;

public class ClearEndpoint : EndpointWithoutRequest
{
    private readonly ILogger<ClearEndpoint> _logger;
    private readonly ICartService _cartService;
    public ClearEndpoint(ILogger<ClearEndpoint> logger, ICartService cartService)
    {
        _logger = logger;
        _cartService = cartService;
    }

    public override void Configure()
    {
        Delete("cart/clear");
        Roles("User");
        Tags("Cart");
        Summary(s => { s.Summary = "Очистить корзину пользователя"; });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var userId = User.GetUserId();
        if (userId == null)
        {
            await SendUnauthorizedAsync(ct);
            return;
        }
        
        var result = await _cartService.ClearCartAsync(userId.Value);
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