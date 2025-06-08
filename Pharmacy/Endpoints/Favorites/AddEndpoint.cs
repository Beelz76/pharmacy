using FastEndpoints;
using Pharmacy.Extensions;
using Pharmacy.Services.Interfaces;

namespace Pharmacy.Endpoints.Favorites;

public class AddEndpoint : EndpointWithoutRequest
{
    private readonly ILogger<AddEndpoint> _logger;
    private readonly IFavoritesService _favoritesService;
    public AddEndpoint(ILogger<AddEndpoint> logger, IFavoritesService favoritesService)
    {
        _logger = logger;
        _favoritesService = favoritesService;
    }

    public override void Configure()
    {
        Post("favorites/{productId:int}");
        Roles("User");
        Tags("Favorites");
        Summary(s => { s.Summary = "Добавить товар в избранное"; });
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
        
        var result = await _favoritesService.AddAsync(userId.Value, productId);
        if (result.IsSuccess)
        {
            await SendOkAsync(ct);
        }
        else
        {
            await SendAsync(result.Error, (int)result.Error.StatusCode, ct);
        }
    }
}