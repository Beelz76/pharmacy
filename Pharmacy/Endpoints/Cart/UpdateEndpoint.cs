using FastEndpoints;
using Pharmacy.Services.Interfaces;

namespace Pharmacy.Endpoints.Cart;

public class UpdateEndpoint : EndpointWithoutRequest
{
    private readonly ILogger<UpdateEndpoint> _logger;
    private readonly IProductService _productService;
    public UpdateEndpoint(ILogger<UpdateEndpoint> logger, IProductService productService)
    {
        _logger = logger;
        _productService = productService;
    }

    public override void Configure()
    {
        Put("cart/{productId:int}");
        Roles("User");
        Tags("Cart");
        Summary(s => { s.Summary = "Обновить количество товара в корзине"; });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await SendOkAsync (ct);
    }
}