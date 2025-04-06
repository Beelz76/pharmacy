using FastEndpoints;

namespace Pharmacy.Endpoints.Authorization;

public class LoginEndpoint : Endpoint<LoginRequest>
{
    private readonly ILogger<LoginEndpoint> _logger;
    
    public LoginEndpoint(ILogger<LoginEndpoint> logger)
    {
        _logger = logger;
    }

    public override void Configure()
    {
        Post("users/login");
        AllowAnonymous();
        Tags("Authorization");
    }

    public override async Task HandleAsync(LoginRequest request, CancellationToken ct)
    {

        await SendOkAsync(ct);
    }
}

public record LoginRequest(string Username, string Password);