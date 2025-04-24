using FastEndpoints;
using Pharmacy.Services.Interfaces;

namespace Pharmacy.Endpoints.Cart;

public class AddEndpoint : Endpoint<AddToCartRequest>
{
    private readonly ILogger<AddEndpoint> _logger;
    private readonly IProductService _productService;
    public AddEndpoint(ILogger<AddEndpoint> logger, IProductService productService)
    {
        _logger = logger;
        _productService = productService;
    }

    public override void Configure()
    {
        Post("cart");
        Roles("User");
        Tags("Cart");
        Summary(s => { s.Summary = "Добавить товар в корзину"; });
    }

    public override async Task HandleAsync(AddToCartRequest request, CancellationToken ct)
    {
        await SendOkAsync (ct);
    }
}

public record AddToCartRequest(string name);