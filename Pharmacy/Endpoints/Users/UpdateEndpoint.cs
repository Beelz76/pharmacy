using FastEndpoints;
using Pharmacy.Services.Interfaces;

namespace Pharmacy.Endpoints.Users;

public class UpdateEndpoint : Endpoint<UpdateUserRequest>
{
    private readonly ILogger<UpdateEndpoint> _logger;
    private readonly IUserService _userService;
    public UpdateEndpoint(ILogger<UpdateEndpoint> logger, IUserService userService)
    {
        _logger = logger;
        _userService = userService;
    }

    public override void Configure()
    {
        Put("users/{id:int}");
        AllowAnonymous();
        Tags("Users");
        Summary(s => { s.Summary = "Изменить данные пользователя"; });
    }

    public override async Task HandleAsync(UpdateUserRequest request, CancellationToken ct)
    {
        var userId = Route<int>("id");
        var result = await _userService.UpdateAsync(userId, request);
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

public record UpdateUserRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string Patronymic,
    string Phone);
