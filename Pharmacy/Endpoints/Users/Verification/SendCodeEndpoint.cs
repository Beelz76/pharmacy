using FastEndpoints;
using FluentValidation;
using Pharmacy.Extensions;
using Pharmacy.Services.Interfaces;
using Pharmacy.Shared.Enums;

namespace Pharmacy.Endpoints.Users.Verification;

public class SendCodeEndpoint : Endpoint<SendCodeRequest>
{
    private readonly ILogger<SendCodeEndpoint> _logger;
    private readonly IEmailVerificationService _emailVerificationService;
    private readonly IUserService _userService;
    public SendCodeEndpoint(ILogger<SendCodeEndpoint> logger, IEmailVerificationService emailVerificationService, IUserService userService)
    {
        _logger = logger;
        _emailVerificationService = emailVerificationService;
        _userService = userService;
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
        if (request.Purpose == VerificationPurposeEnum.EmailChange && !(User.Identity?.IsAuthenticated ?? false))
        {
            await SendUnauthorizedAsync(ct);
            return;
        }

        var userResult = await _userService.GetByEmailAsync(request.Email);
        if (userResult.IsFailure && request.Purpose != VerificationPurposeEnum.EmailChange)
        {
            await SendAsync(userResult.Error, (int)userResult.Error.StatusCode, ct);
            return;
        }
        
        var result = await _emailVerificationService.SendCodeAsync(userResult.ValueOrDefault?.Id ?? User.GetUserId()!.Value, request.Email, userResult.ValueOrDefault?.EmailVerified ?? false, request.Purpose);
        if (result.IsSuccess)
        {
            await SendOkAsync(ct);
        }
        else
        {
            await SendAsync(result.Error, (int)result.Error.StatusCode, ct);
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