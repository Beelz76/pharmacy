using FastEndpoints;
using Pharmacy.Services.Interfaces;

namespace Pharmacy.Endpoints.Users.EmailVerification;

public class SendCodeEndpoint : Endpoint<VerifyEmailRequest>
{
    private readonly ILogger<SendCodeEndpoint> _logger;
    private readonly IAuthorizationService _authorizationService;
    public SendCodeEndpoint(ILogger<SendCodeEndpoint> logger, IAuthorizationService authorizationService)
    {
        _logger = logger;
        _authorizationService = authorizationService;
    }

    public override void Configure()
    {
        Post("email/send-code");
        AllowAnonymous();
        Tags("EmailVerification");
    }

    public override async Task HandleAsync(VerifyEmailRequest request, CancellationToken ct)
    {
        
        await SendOkAsync(ct);
    }
}

public record SendVerificationCodeRequest(string Email, string Code);