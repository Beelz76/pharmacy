using FastEndpoints;
using Pharmacy.Services.Interfaces;

namespace Pharmacy.Endpoints.Payments;

public class GetByOrderEndpoint : Endpoint<CreatePaymentRequest>
{
    private readonly ILogger<GetByOrderEndpoint> _logger;
    private readonly IProductService _productService;
    public GetByOrderEndpoint(ILogger<GetByOrderEndpoint> logger, IProductService productService)
    {
        _logger = logger;
        _productService = productService;
    }

    public override void Configure()
    {
        Get("payments/{orderId:int}");
        AllowAnonymous();
        Tags("Payments");
        Summary(s => { s.Summary = "Получить информацию по оплате заказа"; }); 
    }

    public override async Task HandleAsync(CreatePaymentRequest request, CancellationToken ct)
    {
        await SendOkAsync (ct);
    }
}

public record CreatePaymentRequest(string name);