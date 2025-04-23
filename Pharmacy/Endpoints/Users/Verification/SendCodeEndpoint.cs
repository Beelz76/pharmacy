using FastEndpoints;
using FluentValidation;
using Pharmacy.Services.Interfaces;
using Pharmacy.Shared.Enums;

namespace Pharmacy.Endpoints.Users.Verification;

public class SendCodeEndpoint : Endpoint<SendCodeRequest>
{
    private readonly ILogger<SendCodeEndpoint> _logger;
    private readonly IEmailVerificationService _emailVerificationService;
    public SendCodeEndpoint(ILogger<SendCodeEndpoint> logger, IEmailVerificationService emailVerificationService)
    {
        _logger = logger;
        _emailVerificationService = emailVerificationService;
    }

    public override void Configure()
    {
        Post("verifications/send-code");
        AllowAnonymous();
        Tags("Verifications");
        Summary(s => { s.Summary = "Отправить код подтверждения на email"; });
    }

    public override async Task HandleAsync(SendCodeRequest request, CancellationToken ct)
    {
        if (request.Purpose == VerificationPurposeEnum.EmailChange && !User.Identity?.IsAuthenticated == true)
        {
            await SendUnauthorizedAsync(ct);
            return;
        }

        var result = await _emailVerificationService.SendCodeAsync(request.Email, request.Purpose);
        if (result.IsSuccess)
        {
            await SendOkAsync(ct);
        }
        else
        {
            await SendAsync(result.Error, (int)result.Error.Code, ct);
        }
    }
}

public record SendCodeRequest(string Email, VerificationPurposeEnum Purpose);

public class SendCodeRequestValidator : Validator<SendCodeRequest>
{
    public SendCodeRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.Purpose)
            .NotEmpty();
    }
}