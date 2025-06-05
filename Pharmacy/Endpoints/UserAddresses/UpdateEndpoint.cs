using FastEndpoints;
using Pharmacy.Extensions;
using Pharmacy.Services.Interfaces;

namespace Pharmacy.Endpoints.UserAddresses;

public class UpdateEndpoint : Endpoint<UpdateUserAddressRequest>
{
    private readonly IUserAddressService _service;

    public UpdateEndpoint(IUserAddressService service)
    {
        _service = service;
    }

    public override void Configure()
    {
        Post("users/addresses/{userAddressId:int}");
        Roles("User");
        Tags("Users");
        Summary(s => { s.Summary = "Обновить адрес доставки"; });
    }

    public override async Task HandleAsync(UpdateUserAddressRequest request, CancellationToken ct)
    {
        var userId = User.GetUserId();
        if (userId == null)
        {
            await SendUnauthorizedAsync(ct);
            return;
        }

        var userAddressId = Route<int>("userAddressId");
        
        var result = await _service.UpdateAsync(userId.Value, userAddressId, request);
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

public record UpdateUserAddressRequest(
    string? OsmId,
    string? Region,
    string? State,
    string? City,
    string? Suburb,
    string? Street,
    string? HouseNumber,
    string? Postcode,
    double Latitude,
    double Longitude,
    string? Apartment,
    string? Entrance,
    string? Floor,
    string? Comment
);