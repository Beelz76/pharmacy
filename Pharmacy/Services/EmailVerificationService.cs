using Pharmacy.Database.Entities;
using Pharmacy.Database.Repositories.Interfaces;
using Pharmacy.DateTimeProvider;
using Pharmacy.Endpoints.Users.Authentication;
using Pharmacy.Endpoints.Users.Verification;
using Pharmacy.Extensions;
using Pharmacy.Services.Interfaces;
using Pharmacy.Shared.Dto;
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
    private readonly CodeGenerator _codeGenerator;
    
    private const int CodeLength = 6;
    private const int ExpirationMinutes = 5;

    public EmailVerificationService(TokenProvider tokenProvider, IDateTimeProvider dateTimeProvider, IUserRepository userRepository, PasswordProvider passwordProvider, IEmailVerificationCodeRepository repository, CodeGenerator codeGenerator)
    {
        _tokenProvider = tokenProvider;
        _dateTimeProvider = dateTimeProvider;
        _userRepository = userRepository;
        _passwordProvider = passwordProvider;
        _repository = repository;
        _codeGenerator = codeGenerator;
    }

    public async Task<Result> GenerateVerificationCodeAsync(int userId, string email, VerificationPurposeEnum purpose)
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
            ExpiresAt = now.AddMinutes(ExpirationMinutes)
        };

        await _repository.AddAsync(newCode);
        Console.WriteLine($"[DEBUG] Код подтверждения \"{newCode.Purpose.GetDescription()}\" для {email}: {newCode.Code}");
        return Result.Success();
    }
    
    public async Task<Result> SendCodeAsync(string email, VerificationPurposeEnum purpose)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        if (user is null)
        {
            return Result.Failure<UserDto>(Error.NotFound("Пользователь не найден"));
        }

        if (purpose == VerificationPurposeEnum.Registration && user.EmailVerified)
        {
            return Result.Failure<UserDto>(Error.Conflict("Пользователь уже подтвержден"));
        }
        
        await GenerateVerificationCodeAsync(user.Id, user.Email, purpose);
        
        //TODO отправить код на почту
        
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
                
                var token = _tokenProvider.Create(user.Id, user.Email, user.Role);
                
                return Result.Success(new ConfirmCodeDto(true, token));
            }
            case VerificationPurposeEnum.PasswordReset:
                return Result.Success(new ConfirmCodeDto(true, null));
            case VerificationPurposeEnum.EmailChange:
            {
                var user = await _userRepository.GetByIdAsync(userId!.Value);
                if (user is null)
                {
                    return Result.Failure<ConfirmCodeDto>(Error.NotFound("Пользователь не найден"));
                }
                
                user.Email = email;
                await _userRepository.UpdateAsync(user);

                return Result.Success(new ConfirmCodeDto(true, null));
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
        
        code.ExpiresAt = now;
        await _repository.UpdateAsync(code);

        return Result.Success();
    }
}