using FastEndpoints;
using Pharmacy.Extensions;
using Pharmacy.Services.Interfaces;

namespace Pharmacy.Endpoints.Favorites;

public class ClearEndpoint : EndpointWithoutRequest
{
    private readonly ILogger<ClearEndpoint> _logger;
    private readonly IFavoritesService _favoritesService;
    public ClearEndpoint(ILogger<ClearEndpoint> logger, IFavoritesService favoritesService)
    {
        _logger = logger;
        _favoritesService = favoritesService;
    }

    public override void Configure()
    {
        Delete("favorites");
        Roles("User");
        Tags("Favorites");
        Summary(s => { s.Summary = "Очистить избранное"; });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var userId = User.GetUserId();
        if (userId == null)
        {
            await SendUnauthorizedAsync(ct);
            return;
        }
        
        var result = await _favoritesService.ClearAsync(userId.Value);
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