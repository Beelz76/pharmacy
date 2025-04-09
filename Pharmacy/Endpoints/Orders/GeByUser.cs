using FastEndpoints;
using Pharmacy.Services.Interfaces;

namespace Pharmacy.Endpoints.Orders;

public class GetByUserEndpoint : EndpointWithoutRequest
{
    private readonly ILogger<GetByUserEndpoint> _logger;
    private readonly IProductService _productService;
    public GetByUserEndpoint(ILogger<GetByUserEndpoint> logger, IProductService productService)
    {
        _logger = logger;
        _productService = productService;
    }

    public override void Configure()
    {
        Get("orders/{userId:int}");
        //Roles("Admin");
        AllowAnonymous();
        Tags("Orders");
        Summary(s => { s.Summary = "Получить историю заказов пользователя"; });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await SendOkAsync (ct);
    }
}