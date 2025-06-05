using Pharmacy.Services.Interfaces;
using FastEndpoints;
using Pharmacy.Extensions;

namespace Pharmacy.Endpoints.UserAddresses;

public class GetAllEndpoint : EndpointWithoutRequest
{
    private readonly IUserAddressService _service;

    public GetAllEndpoint(IUserAddressService service)
    {
        _service = service;
    }
        
    public override void Configure()
    {
        Get("users/addresses");
        Roles("User");
        Tags("Users");
        Summary(s => { s.Summary = "Получить все адреса доставки пользователя"; });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var userId = User.GetUserId();
        if (userId == null)
        {
            await SendUnauthorizedAsync(ct);
            return;
        }
        
        var result = await _service.GetAllAsync(userId.Value);

        if (result.IsSuccess)
        {
            await SendOkAsync(result.Value, ct);
        }
        else
        {
            await SendAsync(result.Error, (int)result.Error.Code, cancellation: ct);
        }
    }
}