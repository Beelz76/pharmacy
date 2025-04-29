using FastEndpoints;
using Pharmacy.Services.Interfaces;

namespace Pharmacy.Endpoints.Users;

public class GetByIdEndpoint : EndpointWithoutRequest
{
    private readonly ILogger<GetByIdEndpoint> _logger;
    private readonly IUserService _userService;
    public GetByIdEndpoint(ILogger<GetByIdEndpoint> logger, IUserService userService)
    {
        _logger = logger;
        _userService = userService;
    }

    public override void Configure()
    {
        Get("users/{userId:int}");
        Roles("Admin");
        Tags("Users");
        Summary(s => { s.Summary = "Получить пользователя по id"; });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var userId = Route<int>("id");
        var result = await _userService.GetByIdAsync(userId);
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