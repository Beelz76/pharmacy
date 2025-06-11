using FastEndpoints;
using FluentValidation;
using Pharmacy.Extensions;
using Pharmacy.Services.Interfaces;

namespace Pharmacy.Endpoints.Favorites;

public class AddRangeEndpoint : Endpoint<AddFavoritesRequest>
{
    private readonly IFavoritesService _favoritesService;

    public AddRangeEndpoint(IFavoritesService favoritesService)
    {
        _favoritesService = favoritesService;
    }

    public override void Configure()
    {
        Post("favorites/bulk");
        Roles("User");
        Tags("Favorites");
        Summary(s => { s.Summary = "Добавить несколько товаров в избранное"; });
    }

    public override async Task HandleAsync(AddFavoritesRequest req, CancellationToken ct)
    {
        var userId = User.GetUserId();
        if (userId == null)
        {
            await SendUnauthorizedAsync(ct);
            return;
        }

        var result = await _favoritesService.AddRangeAsync(userId.Value, req.ProductIds);
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

public record AddFavoritesRequest(List<int> ProductIds);

public class AddFavoritesRequestValidator : Validator<AddFavoritesRequest>
{
    public AddFavoritesRequestValidator()
    {
        RuleFor(x => x.ProductIds)
            .NotEmpty();

        RuleForEach(x => x.ProductIds)
            .GreaterThan(0);
    }
}