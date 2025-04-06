using FastEndpoints;

namespace Pharmacy.Endpoints.User;

public record LoginRequest(string Username, string Password);
public record LoginResponse(string Token);

public class LoginEndpoint(ILogger<LoginEndpoint> logger) : Endpoint<LoginRequest, LoginResponse>
{
    public override void Configure()
    {
        Post("/login");
        AllowAnonymous();
    }

    public override async Task HandleAsync(LoginRequest req, CancellationToken ct)
    {
        string token = "sadadas";
        await SendOkAsync(new LoginResponse(token), ct);
    }
}