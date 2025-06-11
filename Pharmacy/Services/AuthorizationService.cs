using Pharmacy.Database;
using Pharmacy.Database.Entities;
using Pharmacy.Database.Repositories.Interfaces;
using Pharmacy.DateTimeProvider;
using Pharmacy.Endpoints.Users.Authentication;
using Pharmacy.Endpoints.Users.Authorization;
using Pharmacy.Services.Interfaces;
using Pharmacy.Shared.Dto.Auth;
using Pharmacy.Shared.Dto.User;
using Pharmacy.Shared.Enums;
using Pharmacy.Shared.Result;

namespace Pharmacy.Services;

public class AuthorizationService : IAuthorizationService
{
    private readonly IUserService _userService;
    private readonly PasswordProvider _passwordProvider;
    private readonly TokenProvider _tokenProvider;
    private readonly IEmailVerificationService _emailVerificationService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly TransactionRunner _transactionRunner;

    public AuthorizationService(IUserService userService, PasswordProvider passwordProvider, TokenProvider tokenProvider, IEmailVerificationService emailVerificationService, IRefreshTokenRepository refreshTokenRepository, IDateTimeProvider dateTimeProvider, TransactionRunner transactionRunner)
    {
        _userService = userService;
        _passwordProvider = passwordProvider;
        _tokenProvider = tokenProvider;
        _emailVerificationService = emailVerificationService;
        _refreshTokenRepository = refreshTokenRepository;
        _dateTimeProvider = dateTimeProvider;
        _transactionRunner = transactionRunner;
    }

    public async Task<Result<string>> RegisterAsync(RegisterRequest request)
    {
        var existingUserResult = await _userService.GetByEmailAsync(request.Email);
        if (existingUserResult.IsSuccess)
        {
            return Result.Failure<string>(Error.Conflict("Пользователь с таким email уже зарегистрирован"));
        }
        
        var passwordHash = _passwordProvider.Hash(request.Password);
        var created = await _userService.CreateAsync(new CreateUserDto(
            request.Email, 
            passwordHash, 
            EmailVerified: false,
            request.FirstName, 
            request.LastName, 
            request.Patronymic, 
            request.Phone, 
            UserRoleEnum.User,
            PharmacyId: null));
        
        var sendResult = await _emailVerificationService.SendCodeAsync(created.Value.Id, request.Email, false, VerificationPurposeEnum.Registration);
        if (sendResult.IsFailure)
        {
            return Result.Failure<string>(sendResult.Error);
        }
        
        return Result.Success("На почту отправлен код подтверждения");
    }
    
    public async Task<Result<LoginResponse>> LoginAsync(LoginRequest request)
    {
        var userResult = await _userService.GetByEmailAsync(request.Email);
        if (userResult.IsFailure)
        {
            return Result.Failure<LoginResponse>(userResult.Error);
        }

        var verifiedPassword = _passwordProvider.Verify(request.Password, userResult.Value.PasswordHash);
        if (!verifiedPassword)
        {
            return Result.Failure<LoginResponse>(Error.Failure("Неверный пароль"));
        }
        
        if (!userResult.Value.EmailVerified)
        {
            var sendResult = await _emailVerificationService.SendCodeAsync(userResult.Value.Id, userResult.Value.Email, userResult.Value.EmailVerified, VerificationPurposeEnum.Registration);
            if (sendResult.IsFailure)
            {
                return Result.Failure<LoginResponse>(sendResult.Error);
            }
            return Result.Success(new LoginResponse("На почту отправлен код подтверждения", null, null));
        }
        
        var jwtId = Guid.NewGuid().ToString();
        var token = _tokenProvider.Create(userResult.Value.Id, userResult.Value.Email, userResult.Value.Role, jwtId);

        var refreshToken = new RefreshToken
        {
            UserId = userResult.Value.Id,
            Token = Guid.NewGuid().ToString(),
            JwtId = jwtId,
            ExpiresAt = _dateTimeProvider.UtcNow.AddDays(30),
            CreatedAt = _dateTimeProvider.UtcNow,
            IsUsed = false
        };
        await _refreshTokenRepository.AddAsync(refreshToken);

        return Result.Success(new LoginResponse(null, token, refreshToken.Token));
    }

    public async Task<Result<LoginResponse>> RefreshAsync(string refreshToken)
    {
        var existing = await _refreshTokenRepository.GetByTokenAsync(refreshToken);
        if (existing == null || existing.IsUsed || existing.ExpiresAt <= _dateTimeProvider.UtcNow)
        {
            return Result.Failure<LoginResponse>(Error.Unauthorized("Неверный токен"));
        }

        var userResult = await _userService.GetByIdAsync(existing.UserId);
        if (userResult.IsFailure)
        {
            return Result.Failure<LoginResponse>(userResult.Error);
        }

        var transactionResult = await _transactionRunner.ExecuteAsync(async () =>
        {
            await _refreshTokenRepository.RemoveAsync(existing);

            var jwtId = Guid.NewGuid().ToString();
            var newToken = _tokenProvider.Create(userResult.Value.Id, userResult.Value.Email, userResult.Value.Role, jwtId);

            var newRefresh = new RefreshToken
            {
                UserId = userResult.Value.Id,
                Token = Guid.NewGuid().ToString(),
                JwtId = jwtId,
                ExpiresAt = _dateTimeProvider.UtcNow.AddDays(30),
                CreatedAt = _dateTimeProvider.UtcNow,
                IsUsed = false
            };

            await _refreshTokenRepository.AddAsync(newRefresh);

            return Result.Success(new LoginResponse(null, newToken, newRefresh.Token));
        });

        return transactionResult;
    }
}