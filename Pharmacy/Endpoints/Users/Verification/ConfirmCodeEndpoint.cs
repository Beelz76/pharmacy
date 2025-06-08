using FastEndpoints;
using FluentValidation;
using Pharmacy.Extensions;
using Pharmacy.Services.Interfaces;
using Pharmacy.Shared.Enums;

namespace Pharmacy.Endpoints.Users.Verification;

public class ConfirmCodeEndpoint : Endpoint<ConfirmCodeRequest>
{
    private readonly ILogger<ConfirmCodeEndpoint> _logger;
    private readonly IEmailVerificationService _emailVerificationService;
    public ConfirmCodeEndpoint(ILogger<ConfirmCodeEndpoint> logger, IAuthorizationService authorizationService, IEmailVerificationService emailVerificationService)
    {
        _logger = logger;
        _emailVerificationService = emailVerificationService;
    }

    public override void Configure()
    {
        Post("verifications/confirm-code");
        AllowAnonymous();
        Tags("Verifications");
        Summary(s => { s.Summary = "Проверить код подтверждения"; });
    }

    public override async Task HandleAsync(ConfirmCodeRequest request, CancellationToken ct)
    {
        int? userId = null;
        if (request.Purpose == VerificationPurposeEnum.EmailChange)
        {
            if (!HttpContext.User.Identity?.IsAuthenticated ?? true)
            {
                await SendUnauthorizedAsync(ct);
                return;
            }
            userId = User.GetUserId();
        }
        
        var result = await _emailVerificationService.ConfirmCodeAsync(request.Email, request.Code, request.Purpose, userId);
        if (result.IsSuccess)
        {
            await SendOkAsync(result.Value, ct);
        }
        else
        {
            await SendAsync(result.Error, (int)result.Error.StatusCode, ct);
        }
    }
}

public record ConfirmCodeRequest(string Email, string Code, VerificationPurposeEnum Purpose);

public class ConfirmCodeRequestValidator : Validator<ConfirmCodeRequest>
{
    public ConfirmCodeRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();
        
        RuleFor(x => x.Code)
            .NotEmpty()
            .Length(6)
            .Matches("^[0-9]+$");

        RuleFor(x => x.Purpose)
            .NotEmpty();
    }
}