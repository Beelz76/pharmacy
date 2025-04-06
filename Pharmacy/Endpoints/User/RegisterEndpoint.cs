using FastEndpoints;
using Pharmacy.Services.Interfaces;

namespace Pharmacy.Endpoints.Authorization;

public class RegisterEndpoint : Endpoint<RegisterRequest>
{
    private readonly ILogger<LoginEndpoint> _logger;
    private readonly IAuthorizationService _authorizationService;
    public RegisterEndpoint(ILogger<LoginEndpoint> logger, IAuthorizationService authorizationService)
    {
        _logger = logger;
        _authorizationService = authorizationService;
    }

    public override void Configure()
    {
        Post("users/register");
        AllowAnonymous();
        Tags("Authorization");
    }

    public override async Task HandleAsync(RegisterRequest request, CancellationToken ct)
    {
        
        await SendOkAsync (ct);
    }
}

public record RegisterRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string? Patronymic = null,
    string? Phone = null);