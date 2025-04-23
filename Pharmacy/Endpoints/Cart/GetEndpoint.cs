using FastEndpoints;
using Pharmacy.Services.Interfaces;

namespace Pharmacy.Endpoints.Cart;

public class GetEndpoint : EndpointWithoutRequest
{
    private readonly ILogger<GetEndpoint> _logger;
    private readonly IProductService _productService;
    public GetEndpoint(ILogger<GetEndpoint> logger, IProductService productService)
    {
        _logger = logger;
        _productService = productService;
    }

    public override void Configure()
    {
        Get("cart/{userId:int}");
        AllowAnonymous();
        Tags("Cart");
        Summary(s => { s.Summary = "Получить корзину пользователя"; });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await SendOkAsync (ct);
    }
}