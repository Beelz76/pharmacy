using FastEndpoints;
using Pharmacy.Extensions;
using Pharmacy.Services.Interfaces;

namespace Pharmacy.Endpoints.UserAddresses;

public class GetByIdEndpoint : EndpointWithoutRequest
{
    private readonly IUserAddressService _service;

    public GetByIdEndpoint(IUserAddressService service)
    {
        _service = service;
    }
        
    public override void Configure()
    {
        Get("users/addresses/{userAddressId:int}");
        Roles("User");
        Tags("Users");
        Summary(s => { s.Summary = "Получить адрес доставки"; });
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
        
        var result = await _service.GetByIdAsync(userId.Value, userAddressId);
        if (result.IsSuccess)
        {
            await SendOkAsync(result.Value, ct);
        }
        else
        {
            await SendAsync(result.Error, (int)result.Error.StatusCode, cancellation: ct);
        }
    }
}