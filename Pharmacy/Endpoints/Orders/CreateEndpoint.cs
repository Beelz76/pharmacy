using FastEndpoints;
using Pharmacy.Services.Interfaces;

namespace Pharmacy.Endpoints.Orders;

public class CreateEndpoint : Endpoint<CreateOrderRequest>
{
    private readonly ILogger<CreateEndpoint> _logger;
    private readonly IProductService _productService;
    public CreateEndpoint(ILogger<CreateEndpoint> logger, IProductService productService)
    {
        _logger = logger;
        _productService = productService;
    }

    public override void Configure()
    {
        Post("orders");
        AllowAnonymous();
        Tags("Orders");
        Summary(s => { s.Summary = "Сформировать заказ"; });
    }

    public override async Task HandleAsync(CreateOrderRequest request, CancellationToken ct)
    {
        await SendOkAsync (ct);
    }
}

public record CreateOrderRequest(string name);