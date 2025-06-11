using Pharmacy.Database.Entities;
using Pharmacy.Database.Repositories;
using Pharmacy.Database.Repositories.Interfaces;
using Pharmacy.DateTimeProvider;
using Pharmacy.Endpoints.Users.Authentication;
using Pharmacy.Endpoints.Users.Verification;
using Pharmacy.Extensions;
using Pharmacy.ExternalServices;
using Pharmacy.Services.Interfaces;
using Pharmacy.Shared.Dto;
using Pharmacy.Shared.Dto.Auth;
using Pharmacy.Shared.Enums;
using Pharmacy.Shared.Result;

namespace Pharmacy.Services;

public class EmailVerificationService : IEmailVerificationService
{
    private readonly TokenProvider _tokenProvider;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IUserRepository _userRepository;
    private readonly PasswordProvider _passwordProvider;
    private readonly IEmailVerificationCodeRepository _repository;
    private readonly IEmailSender _emailSender;
    private readonly CodeGenerator _codeGenerator;
    private readonly IConfiguration _configuration;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    
    private const int CodeLength = 6;

    public EmailVerificationService(TokenProvider tokenProvider, IDateTimeProvider dateTimeProvider, IUserRepository userRepository, PasswordProvider passwordProvider, IEmailVerificationCodeRepository repository, CodeGenerator codeGenerator, IEmailSender emailSender, IConfiguration configuration, IRefreshTokenRepository refreshTokenRepository)
    {
        _tokenProvider = tokenProvider;
        _dateTimeProvider = dateTimeProvider;
        _userRepository = userRepository;
        _passwordProvider = passwordProvider;
        _repository = repository;
        _codeGenerator = codeGenerator;
        _emailSender = emailSender;
        _configuration = configuration;
        _refreshTokenRepository = refreshTokenRepository;
    }

    public async Task<EmailVerificationCode> GenerateVerificationCodeAsync(int userId, string email, VerificationPurposeEnum purpose)
    {
        var now = _dateTimeProvider.UtcNow;
        
        var activeCodes = await _repository.GetAllActiveAsync(userId, purpose, now);
        foreach (var code in activeCodes)
        {
            code.ExpiresAt = now;
        }
        
        var newCode = new EmailVerificationCode
        {
            UserId = userId,
            Email = email,
            Code = _codeGenerator.GenerateDigits(CodeLength),
            Purpose = purpose,
            IsUsed = false,
            ExpiresAt = now.AddMinutes(int.Parse(_configuration["EmailVerificationCodeLifeTimeInMinutes"]!))
        };

        await _repository.AddAsync(newCode);
        Console.WriteLine($"[DEBUG] Код подтверждения \"{newCode.Purpose.GetDescription()}\" для {email}: {newCode.Code}");
        return newCode;
    }
    
    public async Task<Result> SendCodeAsync(int userId, string email, bool emailVerified, VerificationPurposeEnum purpose)
    {
        if (purpose == VerificationPurposeEnum.Registration && emailVerified)
        {
            return Result.Failure(Error.Conflict("Пользователь уже подтвержден"));
        }
        
        var code = await GenerateVerificationCodeAsync(userId, email, purpose);

        var subject = $"Код подтверждения";
        var body = $@"
            <div style="" font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 10px; "">
                <h2 style="" color: #2c3e50; "">Здравствуйте!</h2>
                <p style="" font-size: 16px; color: #333; "">
                    Вы запросили код подтверждения для действия: <strong>{purpose.GetDescription()}</strong>.
                </p>
                <p style="" font-size: 18px; color: #000; margin-top: 20px; "">
                    <strong>Ваш код:</strong>
                </p>
                <div style="" font-size: 28px; font-weight: bold; background-color: #f2f2f2; padding: 15px; text-align: center; border-radius: 6px; margin-bottom: 20px; "">
                    {code.Code}
                </div>
                <p style="" font-size: 14px; color: #666; "">
                    Код действителен до: <strong>{code.ExpiresAt:dd.MM.yyyy HH:mm}</strong>.
                </p>
                <p style="" font-size: 14px; color: #999; "">
                    Если вы не запрашивали этот код, просто проигнорируйте это письмо.
                </p>
            </div>";

        var sendResult = await _emailSender.SendEmailAsync(email, subject, body);
        if (sendResult.IsFailure)
        {
            return sendResult;
        }
        
        return Result.Success();
    }
    
    public async Task<Result<ConfirmCodeDto>> ConfirmCodeAsync(string email, string code, VerificationPurposeEnum purpose, int? userId = null)
    {
        var now = _dateTimeProvider.UtcNow;
        
        var verificationCode = await _repository.GetAsync(email, code, purpose, now);
        if (verificationCode is null)
        {
            return Result.Failure<ConfirmCodeDto>(Error.Failure("Неверный или просроченный код"));
        }

        verificationCode.IsUsed = true;
        await _repository.UpdateAsync(verificationCode);
        
        switch (purpose)
        {
            case VerificationPurposeEnum.Registration:
            {
                var user = await _userRepository.GetByEmailAsync(email);
                if (user is null)
                {
                    return Result.Failure<ConfirmCodeDto>(Error.NotFound("Пользователь не найден"));
                }

                user.EmailVerified = true;
                await _userRepository.UpdateAsync(user);
                
                await _emailSender.SendEmailAsync(user.Email,
                    "Регистрация завершена",
                    $@"<div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 10px;'>
                        <h2 style='color: #2c3e50;'>Здравствуйте, {user.LastName} {user.FirstName}!</h2>
                        <p style='font-size: 16px; color: #333;'>Ваш аккаунт успешно подтвержден.</p>
                    </div>");
                
                var jwtId = Guid.NewGuid().ToString();
                var token = _tokenProvider.Create(user.Id, user.Email, user.Role, jwtId);
                var refresh = new RefreshToken
                {
                    UserId = user.Id,
                    Token = Guid.NewGuid().ToString(),
                    JwtId = jwtId,
                    ExpiresAt = _dateTimeProvider.UtcNow.AddDays(30),
                    CreatedAt = _dateTimeProvider.UtcNow
                };
                await _refreshTokenRepository.AddAsync(refresh);

                return Result.Success(new ConfirmCodeDto(true, token, refresh.Token));
            }
            case VerificationPurposeEnum.PasswordReset:
                return Result.Success(new ConfirmCodeDto(true, null, null));
            case VerificationPurposeEnum.EmailChange:
            {
                var user = await _userRepository.GetByIdAsync(userId!.Value);
                if (user is null)
                {
                    return Result.Failure<ConfirmCodeDto>(Error.NotFound("Пользователь не найден"));
                }
                
                user.Email = email;
                await _userRepository.UpdateAsync(user);

                await _emailSender.SendEmailAsync(user.Email,
                    "Email изменен",
                    $@"<div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 10px;'>
                        <h2 style='color: #2c3e50;'>Здравствуйте, {user.LastName} {user.FirstName}!</h2>
                        <p style='font-size: 16px; color: #333;'>Адрес вашей электронной почты был успешно изменен.</p>
                    </div>");
                
                var jwtId2 = Guid.NewGuid().ToString();
                var token2 = _tokenProvider.Create(user.Id, user.Email, user.Role, jwtId2);
                var refresh2 = new RefreshToken
                {
                    UserId = user.Id,
                    Token = Guid.NewGuid().ToString(),
                    JwtId = jwtId2,
                    ExpiresAt = _dateTimeProvider.UtcNow.AddDays(30),
                    CreatedAt = _dateTimeProvider.UtcNow
                };
                await _refreshTokenRepository.AddAsync(refresh2);

                return Result.Success(new ConfirmCodeDto(true, token2, refresh2.Token));
            }
            default:
                return Result.Failure<ConfirmCodeDto>(Error.Failure("Неподдерживаемая цель кода"));
        }
    }
    
    public async Task<Result<bool>> CheckEmailVerifiedAsync(string email)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        if (user is null)
        {
            return Result.Failure<bool>(Error.NotFound("Пользователь не найден"));
        }

        return Result.Success(user.EmailVerified);
    }
    
    public async Task<Result> RecoverPasswordAsync(string email, string newPassword)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        if (user is null)
        {
            return Result.Failure(Error.NotFound("Пользователь не найден"));
        }
        
        var now = _dateTimeProvider.UtcNow;
        
        var code = await _repository.GetLatestUsedAsync(user.Id, VerificationPurposeEnum.PasswordReset, now);
        if (code is null)
        {
            return Result.Failure(Error.Failure("Подтверждение восстановления пароля не выполнено или код просрочен"));
        }
        
        user.PasswordHash = _passwordProvider.Hash(newPassword);
        await _userRepository.UpdateAsync(user);
        
        await _emailSender.SendEmailAsync(user.Email,
            "Пароль восстановлен",
            $@"<div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 10px;'>
                <h2 style='color: #2c3e50;'>Здравствуйте, {user.LastName} {user.FirstName}!</h2>
                <p style='font-size: 16px; color: #333;'>Ваш пароль был успешно изменен.</p>
            </div>");
        
        code.ExpiresAt = now;
        await _repository.UpdateAsync(code);

        return Result.Success();
    }
}