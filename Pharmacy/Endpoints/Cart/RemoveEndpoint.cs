using FastEndpoints;
using Pharmacy.Services.Interfaces;

namespace Pharmacy.Endpoints.Cart;

public class RemoveEndpoint : EndpointWithoutRequest
{
    private readonly ILogger<RemoveEndpoint> _logger;
    private readonly IProductService _productService;
    public RemoveEndpoint(ILogger<RemoveEndpoint> logger, IProductService productService)
    {
        _logger = logger;
        _productService = productService;
    }

    public override void Configure()
    {
        Delete("cart");
        Roles("User");
        Tags("Cart");
        Summary(s => { s.Summary = "Удалить товар из корзины"; });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await SendOkAsync (ct);
    }
}