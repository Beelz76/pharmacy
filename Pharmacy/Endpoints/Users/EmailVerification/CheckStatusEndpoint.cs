using FastEndpoints;
using Pharmacy.Services.Interfaces;

namespace Pharmacy.Endpoints.Users.EmailVerification;

public class CheckStatusEndpoint : EndpointWithoutRequest
{
    private readonly ILogger<CheckStatusEndpoint> _logger;
    private readonly IAuthorizationService _authorizationService;
    public CheckStatusEndpoint(ILogger<CheckStatusEndpoint> logger, IAuthorizationService authorizationService)
    {
        _logger = logger;
        _authorizationService = authorizationService;
    }

    public override void Configure()
    {
        Get("email/verify/status");
        AllowAnonymous();
        Tags("EmailVerification");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        
        await SendOkAsync(ct);
    }
}
