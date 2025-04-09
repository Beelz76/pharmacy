using FastEndpoints;
using Pharmacy.Services.Interfaces;

namespace Pharmacy.Endpoints.Favorites;

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
        Get("favorites");
        AllowAnonymous();
        Tags("Favorites");
        Summary(s => { s.Summary = "Получить избранное пользователя"; });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await SendOkAsync (ct);
    }
}