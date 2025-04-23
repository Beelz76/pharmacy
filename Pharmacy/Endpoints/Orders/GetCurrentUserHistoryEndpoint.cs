using System.Security.Claims;
using FastEndpoints;
using Pharmacy.Extensions;
using Pharmacy.Services.Interfaces;

namespace Pharmacy.Endpoints.Orders;

public class GetCurrentUserHistoryEndpoint : EndpointWithoutRequest
{
    private readonly ILogger<GetCurrentUserHistoryEndpoint> _logger;
    private readonly IProductService _productService;
    public GetCurrentUserHistoryEndpoint(ILogger<GetCurrentUserHistoryEndpoint> logger, IProductService productService)
    {
        _logger = logger;
        _productService = productService;
    }

    public override void Configure()
    {
        Get("orders/history");
        //AllowAnonymous();
        Tags("Orders");
        Summary(s => { s.Summary = "Получить историю заказов текущего пользователя"; });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        int userId = User.GetUserId();
        await SendOkAsync (ct);
    }
}