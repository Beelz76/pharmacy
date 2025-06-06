using FastEndpoints;
using Pharmacy.Extensions;
using Pharmacy.Services.Interfaces;
using Pharmacy.Shared.Dto;

namespace Pharmacy.Endpoints.UserAddresses;

public class CreateEndpoint : Endpoint<CreateUserAddressRequest>
{
    private readonly IUserAddressService _service;

    public CreateEndpoint(IUserAddressService service)
    {
        _service = service;
    }

    public override void Configure()
    {
        Post("users/address");
        Roles("User");
        Tags("Users");
        Summary(s => { s.Summary = "Добавить адрес доставки"; });
    }

    public override async Task HandleAsync(CreateUserAddressRequest request, CancellationToken ct)
    {
        var userId = User.GetUserId();
        if (userId == null)
        {
            await SendUnauthorizedAsync(ct);
            return;
        }
        
        var result = await _service.CreateAsync(userId.Value, request);
        if (result.IsSuccess)
        {
            await SendOkAsync(result.Value, ct);
        }
        else
        {
            await SendAsync(result.Error, (int)result.Error.Code, ct);
        }
    }
}

public record CreateUserAddressRequest(
    CreateAddressDto Address,
    string? Apartment,
    string? Entrance,
    string? Floor,
    string? Comment
);
