using FastEndpoints;
using Pharmacy.Extensions;
using Pharmacy.Services.Interfaces;
using Pharmacy.Shared.Dto;

namespace Pharmacy.Endpoints.Users;

public class GetProfileEndpoint : EndpointWithoutRequest
{
    private readonly ILogger<GetProfileEndpoint> _logger;
    private readonly IUserService _userService;
    public GetProfileEndpoint(ILogger<GetProfileEndpoint> logger, IUserService userService)
    {
        _logger = logger;
        _userService = userService;
    }

    public override void Configure()
    {
        Get("users/profile");
        Roles("User", "Admin", "Employee");
        Tags("Users", "Admin");
        Summary(s => { s.Summary = "Получить данные профиля текущего пользователя"; });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var userId = User.GetUserId();
        if (userId == null)
        {
            await SendUnauthorizedAsync(ct);
            return;
        }
        
        var result = await _userService.GetByIdAsync(userId.Value);
        if (result.IsFailure)
        {
            await SendAsync(result.Error, (int)result.Error.Code, ct);
        }
        else
        {
            var user = result.Value;
            await SendOkAsync(new UserProfileDto(user.Email, user.FirstName, user.LastName, user.Patronymic, user.Phone, user.Pharmacy), ct);
        }
    }
}