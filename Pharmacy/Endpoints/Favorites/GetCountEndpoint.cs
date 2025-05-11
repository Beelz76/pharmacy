using FastEndpoints;
using Pharmacy.Extensions;
using Pharmacy.Services.Interfaces;

namespace Pharmacy.Endpoints.Favorites;

public class GetCountEndpoint : EndpointWithoutRequest
{
    private readonly ILogger<GetCountEndpoint> _logger;
    private readonly IFavoritesService _favoritesService;
    public GetCountEndpoint(ILogger<GetCountEndpoint> logger, IFavoritesService favoritesService)
    {
        _logger = logger;
        _favoritesService = favoritesService;
    }

    public override void Configure()
    {
        Get("favorites/count");
        Roles("User");
        Tags("Favorites");
        Summary(s => { s.Summary = "Получить количество избранного пользователя"; });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var userId = User.GetUserId();
        if (userId == null)
        {
            await SendUnauthorizedAsync(ct);
            return;
        }
        
        var result = await _favoritesService.GetCountAsync(userId.Value);
        if (result.IsSuccess)
        {
            await SendOkAsync(result.Value, ct);
        }
    }
}