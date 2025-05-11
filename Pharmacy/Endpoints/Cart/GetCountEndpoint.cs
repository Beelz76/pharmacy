using FastEndpoints;
using Pharmacy.Extensions;
using Pharmacy.Services.Interfaces;

namespace Pharmacy.Endpoints.Cart;

public class GetCountEndpoint : EndpointWithoutRequest
{
    private readonly ILogger<GetCountEndpoint> _logger;
    private readonly ICartService _cartService;
    public GetCountEndpoint(ILogger<GetCountEndpoint> logger, ICartService cartService)
    {
        _logger = logger;
        _cartService = cartService;
    }

    public override void Configure()
    {
        Get("cart/count");
        Roles("User");
        Tags("Cart");
        Summary(s => { s.Summary = "Получить количество товаров в корзине текущего пользователя"; });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var userId = User.GetUserId();
        if (userId == null)
        {
            await SendUnauthorizedAsync(ct);
            return;
        }
        
        var result = await _cartService.GetCountAsync(userId.Value);
        if (result.IsSuccess)
        {
            await SendOkAsync(result.Value, ct);
        }
    }
}