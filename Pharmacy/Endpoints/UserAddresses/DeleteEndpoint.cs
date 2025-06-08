using FastEndpoints;
using Pharmacy.Extensions;
using Pharmacy.Services.Interfaces;

namespace Pharmacy.Endpoints.UserAddresses;

public class DeleteEndpoint : EndpointWithoutRequest
{
    private readonly IUserAddressService _service;

    public DeleteEndpoint(IUserAddressService service)
    {
        _service = service;
    }

    public override void Configure()
    {
        Delete("users/addresses/{userAddressId:int}");
        Roles("User");
        Tags("Users");
        Summary(s => { s.Summary = "Удалить адрес доставки"; });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var userId = User.GetUserId();
        if (userId == null)
        {
            await SendUnauthorizedAsync(ct);
            return;
        }

        var userAddressId = Route<int>("userAddressId");

        var result = await _service.DeleteAsync(userId.Value, userAddressId);
        if (!result.IsSuccess)
        {
            await SendAsync(result.Error, (int)result.Error.StatusCode, ct);
        }
        else
        {
            await SendOkAsync(ct);
        }
    }
}