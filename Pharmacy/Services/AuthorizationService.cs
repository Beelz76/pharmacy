using Pharmacy.Endpoints.Users.Authentication;
using Pharmacy.Endpoints.Users.Authorization;
using Pharmacy.ExternalServices;
using Pharmacy.Services.Interfaces;
using Pharmacy.Shared.Dto;
using Pharmacy.Shared.Enums;
using Pharmacy.Shared.Result;

namespace Pharmacy.Services;

public class AuthorizationService : IAuthorizationService
{
    private readonly IUserService _userService;
    private readonly IEmailService _emailService;
    private readonly PasswordProvider _passwordProvider;
    private readonly TokenProvider _tokenProvider;
    private readonly IEmailVerificationService _emailVerificationService;

    public AuthorizationService(IUserService userService, PasswordProvider passwordProvider, TokenProvider tokenProvider, IEmailService emailService, IEmailVerificationService emailVerificationService)
    {
        _userService = userService;
        _passwordProvider = passwordProvider;
        _tokenProvider = tokenProvider;
        _emailService = emailService;
        _emailVerificationService = emailVerificationService;
    }

    public async Task<Result<string>> RegisterAsync(RegisterRequest request)
    {
        var existingUserResult = await _userService.GetByEmailAsync(request.Email);
        if (existingUserResult.IsSuccess)
        {
            return Result.Failure<string>(Error.Conflict("Пользователь с таким email уже зарегистрирован"));
        }
        
        var passwordHash = _passwordProvider.Hash(request.Password);
        await _userService.CreateAsync(new CreateUserDto(
            request.Email, 
            passwordHash, 
            false,
            request.FirstName, 
            request.LastName, 
            request.Patronymic, 
            request.Phone, 
            UserRoleEnum.User));
        
        var sendResult = await _emailVerificationService.SendCodeAsync(request.Email, VerificationPurposeEnum.Registration);
        if (sendResult.IsFailure)
        {
            return Result.Failure<string>(sendResult.Error);
        }
        
        return Result.Success<string>("На почту отправлен код подтверждения");
    }
    
    public async Task<Result<string>> LoginAsync(LoginRequest request)
    {
        var userResult = await _userService.GetByEmailAsync(request.Email);
        if (userResult.IsFailure)
        {
            return Result.Failure<string>(userResult.Error);
        }

        var verifiedPassword = _passwordProvider.Verify(request.Password, userResult.Value.PasswordHash);
        if (!verifiedPassword)
        {
            return Result.Failure<string>(Error.Failure("Неверный пароль"));
        }
        
        if (!userResult.Value.EmailVerified)
        {
            var sendResult = await _emailVerificationService.SendCodeAsync(request.Email, VerificationPurposeEnum.Registration);
            if (sendResult.IsFailure)
            {
                return Result.Failure<string>(sendResult.Error);
            }
            return Result.Success<string>("На почту отправлен код подтверждения");
        }
        
        var token = _tokenProvider.Create(userResult.Value.Id, userResult.Value.Email, userResult.Value.Role);
        
        return Result.Success(token);
    }
}