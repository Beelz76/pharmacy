using FastEndpoints;
using Pharmacy.Services.Interfaces;

namespace Pharmacy.Endpoints.Users;

public class GetAllEndpoint : EndpointWithoutRequest
{
    private readonly ILogger<GetAllEndpoint> _logger;
    private readonly IUserService _userService;
    public GetAllEndpoint(ILogger<GetAllEndpoint> logger, IUserService userService)
    {
        _logger = logger;
        _userService = userService;
    }

    public override void Configure()
    {
        Get("users");
        AllowAnonymous();
        Tags("Users");
        Summary(s => { s.Summary = "Получить всех пользователей"; });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await SendOkAsync(ct);
    }
}