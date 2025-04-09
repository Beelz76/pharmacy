using FastEndpoints;
using Pharmacy.Services.Interfaces;

namespace Pharmacy.Endpoints.Orders;

public class GetByIdEndpoint : EndpointWithoutRequest
{
    private readonly ILogger<GetByIdEndpoint> _logger;
    private readonly IProductService _productService;
    public GetByIdEndpoint(ILogger<GetByIdEndpoint> logger, IProductService productService)
    {
        _logger = logger;
        _productService = productService;
    }

    public override void Configure()
    {
        Get("orders/{id:int}");
        AllowAnonymous();
        Tags("Orders");
        Summary(s => { s.Summary = "Получить заказ по id"; });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await SendOkAsync (ct);
    }
}