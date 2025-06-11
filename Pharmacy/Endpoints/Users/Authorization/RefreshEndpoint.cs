using FastEndpoints;
using FluentValidation;
using Pharmacy.Services.Interfaces;

namespace Pharmacy.Endpoints.Users.Authorization;

public class RefreshEndpoint : Endpoint<RefreshRequest>
{
    private readonly IAuthorizationService _authService;

    public RefreshEndpoint(IAuthorizationService authService)
    {
        _authService = authService;
    }

    public override void Configure()
    {
        Post("authorization/refresh");
        AllowAnonymous();
        Tags("Authorization");
        Summary(s => s.Summary = "Обновление токена");
    }

    public override async Task HandleAsync(RefreshRequest req, CancellationToken ct)
    {
        var result = await _authService.RefreshAsync(req.RefreshToken);
        if (result.IsSuccess)
            await SendOkAsync(result.Value, ct);
        else
            await SendAsync(result.Error, (int)result.Error.StatusCode, ct);
    }
}

public record RefreshRequest(string RefreshToken);

public class RefreshRequestValidator : Validator<RefreshRequest>
{
    public RefreshRequestValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty();
    }
}