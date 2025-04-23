using System.Net;
using Pharmacy.Database.Entities;
using Pharmacy.Database.Repositories.Interfaces;
using Pharmacy.DateTimeProvider;
using Pharmacy.Endpoints.Users;
using Pharmacy.Endpoints.Users.Authentication;
using Pharmacy.Services.Interfaces;
using Pharmacy.Shared.Dto;
using Pharmacy.Shared.Enums;
using Pharmacy.Shared.Result;

namespace Pharmacy.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _repository;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly PasswordProvider _passwordProvider;
    private readonly IEmailVerificationService _emailVerificationService;
    public UserService(IUserRepository repository, IDateTimeProvider dateTimeProvider, PasswordProvider passwordProvider, IEmailVerificationService emailVerificationService)
    {
        _repository = repository;
        _dateTimeProvider = dateTimeProvider;
        _passwordProvider = passwordProvider;
        _emailVerificationService = emailVerificationService;
    }

    public async Task<Result<int>> CreateAsync(CreateUserDto dto)
    {
        var user = new User
        {
            Email = dto.Email,
            PasswordHash = dto.PasswordHash,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Patronymic = dto.Patronymic,
            Phone = dto.Phone,
            EmailVerified = dto.EmailVerified,
            CreatedAt = _dateTimeProvider.UtcNow,
            UpdatedAt = _dateTimeProvider.UtcNow
        };
        
        await _repository.AddAsync(user);
        return Result.Success(user.Id);
    }

    public async Task<Result> UpdateAsync(int id, UpdateUserRequest request)
    {
        return await _repository.ExecuteInTransactionAsync(async () =>
        {
            var user = await _repository.GetByIdAsync(id);
            if (user is null)
            {
                return Result.Failure(HttpStatusCode.NotFound, ErrorTypeEnum.NotFound, "Пользователь не найден");
            }

            if (user.Email != request.Email)
            {
                var existingUser = await _repository.GetByEmailAsync(request.Email, excludeId: id);
                if (existingUser is not null)
                {
                    return Result.Failure(Error.Conflict("Пользователь с таким email уже зарегистрирован"));
                }
                
                //TODO Выслать код на почту
            }

            user.Email = request.Email;
            user.PasswordHash = _passwordProvider.Hash(request.Password);
            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.Patronymic = request.Patronymic;
            user.Phone = request.Phone;
            user.UpdatedAt = _dateTimeProvider.UtcNow;

            await _repository.UpdateAsync(user);
            return Result.Success();
        });
    }

    public async Task<Result<UserDto>> GetByIdAsync(int id)
    {
        var user = await _repository.GetByIdAsync(id);
        if (user is null)
        {
            return Result.Failure<UserDto>(Error.NotFound("Пользователь не найден"));
        }

        return Result.Success(new UserDto(user.Id, user.Email, user.PasswordHash, user.EmailVerified, user.FirstName,
            user.LastName, user.Patronymic, user.Phone));
    }

    public async Task<Result<UserDto>> GetByEmailAsync(string email)
    {
        var user = await _repository.GetByEmailAsync(email);
        if (user is null)
        {
            return Result.Failure<UserDto>(Error.NotFound("Пользователь не найден"));
        }
        
        return Result.Success(new UserDto(user.Id, user.Email, user.PasswordHash, user.EmailVerified, user.FirstName,
            user.LastName, user.Patronymic, user.Phone));
    }
    
    public async Task<Result> SetEmailVerifiedAsync(int userId)
    {
        var user = await _repository.GetByIdAsync(userId);
        if (user is null)
        {
            return Result.Failure(Error.NotFound("Пользователь не найден"));
        }

        user.EmailVerified = true;
        await _repository.UpdateAsync(user);
        return Result.Success();
    }
    
    public async Task<Result> SetPasswordAsync(int userId, string newPasswordHash)
    {
        var user = await _repository.GetByIdAsync(userId);
        if (user is null)
        {
            return Result.Failure(Error.NotFound("Пользователь не найден"));
        }

        user.PasswordHash = newPasswordHash;
        await _repository.UpdateAsync(user);
        return Result.Success();
    }
    
    public async Task<Result> UpdateEmailAsync(int userId, string newEmail)
    {
        var user = await _repository.GetByEmailAsync(newEmail);
        if (user is not null)
        {
            return Result.Failure(Error.Conflict("Пользователь с таким email уже зарегистрирован"));
        }
        
        return await _emailVerificationService.GenerateVerificationCodeAsync(userId, newEmail, VerificationPurposeEnum.EmailChange);
    }
    
    public async Task<Result> UpdatePasswordAsync(int userId, string currentPassword, string newPassword)
    {
        var user = await _repository.GetByIdAsync(userId);
        if (user is null)
        {
            return Result.Failure(Error.NotFound("Пользователь не найден"));
        }

        if (!_passwordProvider.Verify(currentPassword, user.PasswordHash))
        {
            return Result.Failure(Error.Failure("Неверный текущий пароль"));
        }

        user.PasswordHash = _passwordProvider.Hash(newPassword);
        user.UpdatedAt = _dateTimeProvider.UtcNow;

        await _repository.UpdateAsync(user);
        return Result.Success();
    }
}