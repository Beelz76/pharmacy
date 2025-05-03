using FastEndpoints;
using Pharmacy.Extensions;
using Pharmacy.Services.Interfaces;

namespace Pharmacy.Endpoints.Cart;

public class GetEndpoint : EndpointWithoutRequest
{
    private readonly ILogger<GetEndpoint> _logger;
    private readonly ICartService _cartService;
    public GetEndpoint(ILogger<GetEndpoint> logger, ICartService cartService)
    {
        _logger = logger;
        _cartService = cartService;
    }

    public override void Configure()
    {
        Get("cart");
        Roles("User");
        Tags("Cart");
        Summary(s => { s.Summary = "Получить корзину текущего пользователя"; });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var userId = User.GetUserId();
        
        var result = await _cartService.GetByUserAsync(userId);
        if (result.IsSuccess)
        {
            await SendOkAsync(result.Value, ct);
        }
    }
}