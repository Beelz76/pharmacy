using FastEndpoints;
using Pharmacy.Extensions;
using Pharmacy.Services.Interfaces;

namespace Pharmacy.Endpoints.Favorites;

public class GetEndpoint : EndpointWithoutRequest
{
    private readonly ILogger<GetEndpoint> _logger;
    private readonly IFavoritesService _favoritesService;
    public GetEndpoint(ILogger<GetEndpoint> logger, IFavoritesService favoritesService)
    {
        _logger = logger;
        _favoritesService = favoritesService;
    }

    public override void Configure()
    {
        Get("favorites");
        Roles("User");
        Tags("Favorites");
        Summary(s => { s.Summary = "Получить избранное пользователя"; });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var userId = User.GetUserId();
        
        var result = await _favoritesService.GetByUserAsync(userId);
        if (result.IsSuccess)
        {
            await SendOkAsync(result.Value, ct);
        }
    }
}