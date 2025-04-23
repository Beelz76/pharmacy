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
    private readonly IUserService _userService;
    private readonly PasswordProvider _passwordProvider;
    private readonly IEmailVerificationCodeRepository _repository;
    private readonly CodeGenerator _codeGenerator;
    
    private const int CodeLength = 6;
    private const int ExpirationMinutes = 1;

    public EmailVerificationService(TokenProvider tokenProvider, IDateTimeProvider dateTimeProvider, IUserService userService, PasswordProvider passwordProvider, IEmailVerificationCodeRepository repository, CodeGenerator codeGenerator)
    {
        _tokenProvider = tokenProvider;
        _dateTimeProvider = dateTimeProvider;
        _userService = userService;
        _passwordProvider = passwordProvider;
        _repository = repository;
        _codeGenerator = codeGenerator;
    }

    public async Task<Result> GenerateVerificationCodeAsync(int userId, string email, VerificationPurposeEnum purpose)
    {
        var now = _dateTimeProvider.UtcNow;
        
        var activeCodes = await _repository.GetAllActiveAsync(email, purpose, now);
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
        var userResult = await _userService.GetByEmailAsync(email);
        if (userResult.IsFailure)
        {
            return Result.Failure(userResult.Error);
        }

        await GenerateVerificationCodeAsync(userResult.Value.Id, userResult.Value.Email, purpose);
        
        //TODO отправить код на почту
        
        return Result.Success();
    }
    
    public async Task<Result<ConfirmCodeDto>> ConfirmCodeAsync(string email, string code, VerificationPurposeEnum purpose, int? userId = null)
    {
        var now = _dateTimeProvider.UtcNow;
        
        var record = await _repository.GetAsync(email, code, purpose, now);
        if (record is null)
        {
            return Result.Failure<ConfirmCodeDto>(Error.Failure("Неверный или просроченный код"));
        }

        record.IsUsed = true;
        await _repository.UpdateAsync(record);
        
        switch (purpose)
        {
            case VerificationPurposeEnum.Registration:
            {
                var userResult = await _userService.GetByEmailAsync(email);
                if (userResult.IsFailure)
                {
                    return Result.Failure<ConfirmCodeDto>(userResult.Error);
                }

                await _userService.SetEmailVerifiedAsync(userResult.Value.Id);
                
                var token = _tokenProvider.Create(userResult.Value.Id, userResult.Value.Email);
                
                return Result.Success(new ConfirmCodeDto(true, token));
            }
            case VerificationPurposeEnum.PasswordReset:
                return Result.Success(new ConfirmCodeDto(true, null));
            case VerificationPurposeEnum.EmailChange:
                var updateResult = await _userService.UpdateEmailAsync(userId!.Value, email);
                if (updateResult.IsFailure)
                {
                    return Result.Failure<ConfirmCodeDto>(updateResult.Error);
                }
                return Result.Success(new ConfirmCodeDto(true, null));
            default:
                return Result.Failure<ConfirmCodeDto>(Error.Failure("Неподдерживаемая цель кода"));
        }
    }
    
    public async Task<Result<bool>> CheckEmailVerifiedAsync(string email)
    {
        var userResult = await _userService.GetByEmailAsync(email);
        if (userResult.IsFailure)
        {
            return Result.Failure<bool>(userResult.Error);
        }

        return Result.Success(userResult.Value.EmailVerified);
    }
    
    public async Task<Result> RecoverPasswordAsync(string email, string newPassword)
    {
        var code = await _repository.GetLatestUsedAsync(email, VerificationPurposeEnum.PasswordReset, _dateTimeProvider.UtcNow);
        if (code is null)
        {
            return Result.Failure(Error.Failure("Подтверждение восстановления пароля не выполнено"));
        }

        var userResult = await _userService.GetByEmailAsync(email);
        if (userResult.IsFailure)
        {
            return Result.Failure<ConfirmCodeDto>(userResult.Error);
        }

        var passwordHash = _passwordProvider.Hash(newPassword);
        return await _userService.SetPasswordAsync(userResult.Value.Id, passwordHash);;
    }
}