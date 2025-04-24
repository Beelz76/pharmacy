using FastEndpoints;
using Pharmacy.Services.Interfaces;

namespace Pharmacy.Endpoints.Favorites;

public class RemoveEndpoint : Endpoint<RemoveFromFavoritesRequest>
{
    private readonly ILogger<RemoveEndpoint> _logger;
    private readonly IProductService _productService;
    public RemoveEndpoint(ILogger<RemoveEndpoint> logger, IProductService productService)
    {
        _logger = logger;
        _productService = productService;
    }

    public override void Configure()
    {
        Delete("favorites");
        Roles("User");
        Tags("Favorites");
        Summary(s => { s.Summary = "Удалить товар из избранного"; });
    }

    public override async Task HandleAsync(RemoveFromFavoritesRequest request, CancellationToken ct)
    {
        await SendOkAsync (ct);
    }
}

public record RemoveFromFavoritesRequest(string name);