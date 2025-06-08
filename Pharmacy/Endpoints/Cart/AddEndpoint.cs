using FastEndpoints;
using FluentValidation;
using Pharmacy.Extensions;
using Pharmacy.Services.Interfaces;

namespace Pharmacy.Endpoints.Cart;

public class AddEndpoint : EndpointWithoutRequest
{
    private readonly ILogger<AddEndpoint> _logger;
    private readonly ICartService _cartService;
    public AddEndpoint(ILogger<AddEndpoint> logger, ICartService cartService)
    {
        _logger = logger;
        _cartService = cartService;
    }

    public override void Configure()
    {
        Post("cart/{productId:int}");
        Roles("User");
        Tags("Cart");
        Summary(s => { s.Summary = "Добавить товар в корзину / увеличить количество товара в корзине"; });
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
        
        var result = await _cartService.AddToCartAsync(userId.Value, productId);
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