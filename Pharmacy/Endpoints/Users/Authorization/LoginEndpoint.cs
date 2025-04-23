using FastEndpoints;
using FluentValidation;
using Pharmacy.Services.Interfaces;

namespace Pharmacy.Endpoints.Users.Authorization;

public class LoginEndpoint : Endpoint<LoginRequest>
{
    private readonly ILogger<LoginEndpoint> _logger;
    private readonly IAuthorizationService _authorizationService;
    public LoginEndpoint(ILogger<LoginEndpoint> logger, IAuthorizationService authorizationService)
    {
        _logger = logger;
        _authorizationService = authorizationService;
    }

    public override void Configure()
    {
        Post("authorization/login");
        AllowAnonymous();
        Tags("Authorization");
        Summary(s => { s.Summary = "Вход"; });
    }

    public override async Task HandleAsync(LoginRequest request, CancellationToken ct)
    {
        var result = await _authorizationService.LoginAsync(request);

        if (result.IsSuccess)
        {
            await SendStringAsync(result.Value, cancellation: ct);
        }
        else
        {
            await SendAsync(result.Error, (int)result.Error.Code, ct);
        }
    }
}

public record LoginRequest(string Email, string Password);
public record LoginResponse(string Token);

public class LoginRequestRequestValidator : Validator<LoginRequest>
{
    public LoginRequestRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.Password)
            .NotEmpty();
    }
}