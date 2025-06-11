using FastEndpoints;
using FluentValidation;
using Pharmacy.Extensions;
using Pharmacy.Services.Interfaces;
using Pharmacy.Shared.Dto.Address;

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
            await SendAsync(result.Error, (int)result.Error.StatusCode, ct);
        }
    }
}

public record UpdateUserAddressRequest(
    CreateAddressDto Address,
    string? Apartment,
    string? Entrance,
    string? Floor,
    string? Comment
);

public class UpdateUserAddressRequestValidator : Validator<UpdateUserAddressRequest>
{
    public UpdateUserAddressRequestValidator()
    {
        RuleFor(x => x.Address).NotNull();
        RuleFor(x => x.Address.Latitude).NotEmpty();
        RuleFor(x => x.Address.Longitude).NotEmpty();
    }
}