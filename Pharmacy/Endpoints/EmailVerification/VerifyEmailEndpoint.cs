using FastEndpoints;
using FluentValidation;
using Pharmacy.Services.Interfaces;

namespace Pharmacy.Endpoints.Authorization;

public class VerifyEmailEndpoint : Endpoint<VerifyEmailRequest>
{
    private readonly ILogger<VerifyEmailEndpoint> _logger;
    private readonly IAuthorizationService _authorizationService;
    public VerifyEmailEndpoint(ILogger<VerifyEmailEndpoint> logger, IAuthorizationService authorizationService)
    {
        _logger = logger;
        _authorizationService = authorizationService;
    }

    public override void Configure()
    {
        Post("users/verify-email");
        AllowAnonymous();
        Tags("Authorization");
    }

    public override async Task HandleAsync(VerifyEmailRequest request, CancellationToken ct)
    {
        
        await SendOkAsync(ct);
    }
}

public record VerifyEmailRequest(string Email, string Code);

public class VerifyEmailRequestValidator : Validator<VerifyEmailRequest>
{
    public VerifyEmailRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();
        
        RuleFor(x => x.Code)
            .NotEmpty()
            .Length(6)
            .Matches("^[0-9]+$");
    }
}