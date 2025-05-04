using FastEndpoints;
using Pharmacy.Extensions;
using Pharmacy.Services.Interfaces;

namespace Pharmacy.Endpoints.Favorites;

public class RemoveEndpoint : EndpointWithoutRequest
{
    private readonly ILogger<RemoveEndpoint> _logger;
    private readonly IFavoritesService _favoritesService;
    public RemoveEndpoint(ILogger<RemoveEndpoint> logger, IFavoritesService favoritesService)
    {
        _logger = logger;
        _favoritesService = favoritesService;
    }

    public override void Configure()
    {
        Delete("favorites/{productId:int}");
        Roles("User");
        Tags("Favorites");
        Summary(s => { s.Summary = "Удалить товар из избранного"; });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var userId = User.GetUserId();
        if (userId == null)
        {
            await SendUnauthorizedAsync(ct);
            return;
        }
        var productId = Route<int>("productId");
        
        var result = await _favoritesService.RemoveAsync(userId.Value, productId);
        if (result.IsSuccess)
        {
            await SendOkAsync(ct);
        }
        else
        {
            await SendAsync(result.Error, (int)result.Error.Code, ct);
        }
    }
}
