using FastEndpoints;
using FluentValidation;
using Pharmacy.Services.Interfaces;

namespace Pharmacy.Endpoints.Users.Authorization;

public class FinalizePasswordRecovery : Endpoint<RecoveryPasswordRequest>
{
    private readonly ILogger<FinalizePasswordRecovery> _logger;
    private readonly IEmailVerificationService _emailVerificationService;
    public FinalizePasswordRecovery(ILogger<FinalizePasswordRecovery> logger, IEmailVerificationService emailVerificationService)
    {
        _logger = logger;
        _emailVerificationService = emailVerificationService;
    }

    public override void Configure()
    {
        Post("authorization/reset-password");
        AllowAnonymous();
        Tags("Authorization");
        Summary(s => { s.Summary = "Финальный шаг восстановления пароля"; });
    }

    public override async Task HandleAsync(RecoveryPasswordRequest request, CancellationToken ct)
    {
        var result = await _emailVerificationService.RecoverPasswordAsync(request.Email, request.NewPassword);
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

public record RecoveryPasswordRequest(string Email, string NewPassword, string ConfirmPassword);

public class SendCodeRequestValidator : Validator<RecoveryPasswordRequest>
{
    public SendCodeRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.NewPassword)
            .NotEmpty();
        
        RuleFor(x => x.ConfirmPassword)
            .NotEmpty()
            .Equal(x => x.NewPassword);
    }
}