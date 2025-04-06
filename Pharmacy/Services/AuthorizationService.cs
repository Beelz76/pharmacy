using Pharmacy.Data.Authorization;
using Pharmacy.Endpoints.Authorization;
using Pharmacy.Services.Interfaces;

namespace Pharmacy.Services;

public class AuthorizationService : IAuthorizationService
{
    private readonly IUserService _userService;
    private readonly IEmailService _emailService;
    private readonly PasswordProvider _passwordProvider;
    private readonly TokenProvider _tokenProvider;

    public AuthorizationService(IUserService userService, PasswordProvider passwordProvider, TokenProvider tokenProvider, IEmailService emailService)
    {
        _userService = userService;
        _passwordProvider = passwordProvider;
        _tokenProvider = tokenProvider;
        _emailService = emailService;
    }

    public async Task<string> LoginAsync(LoginRequest request)
    {
        
    }
}