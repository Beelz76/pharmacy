using FastEndpoints;
using Pharmacy.Services.Interfaces;

namespace Pharmacy.Endpoints.Orders;

public class UpdateStatusEndpoint : EndpointWithoutRequest
{
    private readonly ILogger<UpdateStatusEndpoint> _logger;
    private readonly IProductService _productService;
    public UpdateStatusEndpoint(ILogger<UpdateStatusEndpoint> logger, IProductService productService)
    {
        _logger = logger;
        _productService = productService;
    }

    public override void Configure()
    {
        Get("orders/{orderId:int}/status");
        Roles("Admin", "Employee");
        Tags("Orders");
        Summary(s => { s.Summary = "Обновить статус заказа"; });;
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await SendOkAsync (ct);
    }
}