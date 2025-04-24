using FastEndpoints;
using FluentValidation;
using Pharmacy.Services.Interfaces;

namespace Pharmacy.Endpoints.Users.Verification;

public class CheckEmailVerifiedEndpoint : Endpoint<CheckEmailVerifiedRequest>
{
    private readonly ILogger<CheckEmailVerifiedEndpoint> _logger;
    private readonly IEmailVerificationService _emailVerificationService;
    public CheckEmailVerifiedEndpoint(ILogger<CheckEmailVerifiedEndpoint> logger, IEmailVerificationService emailVerificationService)
    {
        _emailVerificationService = emailVerificationService;
        _logger = logger;
    }

    public override void Configure()
    {
        Get("verifications/check-email");
        Roles("Admin");
        Tags("Verifications");
        Summary(s => { s.Summary = "Проверить статус подтверждения почты"; });
    }

    public override async Task HandleAsync(CheckEmailVerifiedRequest request, CancellationToken ct)
    {
        var result = await _emailVerificationService.CheckEmailVerifiedAsync(request.Email);
        await SendOkAsync(result.Value, ct);
    }
}

public record CheckEmailVerifiedRequest(string Email);

public class CheckEmailVerifiedRequestValidator : Validator<CheckEmailVerifiedRequest>
{
    public CheckEmailVerifiedRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();
    }
}