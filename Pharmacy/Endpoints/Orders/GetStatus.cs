using FastEndpoints;
using Pharmacy.Services.Interfaces;

namespace Pharmacy.Endpoints.Orders;

public class GetStatusEndpoint : EndpointWithoutRequest
{
    private readonly ILogger<GetStatusEndpoint> _logger;
    private readonly IProductService _productService;
    public GetStatusEndpoint(ILogger<GetStatusEndpoint> logger, IProductService productService)
    {
        _logger = logger;
        _productService = productService;
    }

    public override void Configure()
    {
        Get("orders/{id:int}/status");
        //Roles("Admin");
        AllowAnonymous();
        Tags("Orders");
        Summary(s => { s.Summary = "Получить статус заказа"; });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await SendOkAsync (ct);
    }
}