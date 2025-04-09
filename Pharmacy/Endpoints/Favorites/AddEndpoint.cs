using FastEndpoints;
using Pharmacy.Services.Interfaces;

namespace Pharmacy.Endpoints.Favorites;

public class AddEndpoint : Endpoint<AddToFavoritesRequest>
{
    private readonly ILogger<AddEndpoint> _logger;
    private readonly IProductService _productService;
    public AddEndpoint(ILogger<AddEndpoint> logger, IProductService productService)
    {
        _logger = logger;
        _productService = productService;
    }

    public override void Configure()
    {
        Post("favorites");
        AllowAnonymous();
        Tags("Favorites");
        Summary(s => { s.Summary = "Добавить товар в избранное"; });
    }

    public override async Task HandleAsync(AddToFavoritesRequest request, CancellationToken ct)
    {
        await SendOkAsync (ct);
    }
}

public record AddToFavoritesRequest(string name);