using Microsoft.EntityFrameworkCore;
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

    public async Task<Result<CreatedDto>> CreateAsync(CreateUserDto dto)
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
            Role = dto.Role,
            CreatedAt = _dateTimeProvider.UtcNow,
            UpdatedAt = _dateTimeProvider.UtcNow
        };
        
        await _repository.AddAsync(user);
        return Result.Success(new CreatedDto(user.Id));
    }

    public async Task<Result> UpdateProfileAsync(int id, UpdateProfileRequest request)
    {
        var user = await _repository.GetByIdAsync(id);
        if (user is null)
        {
            return Result.Failure(Error.NotFound("Пользователь не найден"));
        }

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.Patronymic = request.Patronymic;
        user.Phone = request.Phone;
        user.UpdatedAt = _dateTimeProvider.UtcNow;

        await _repository.UpdateAsync(user);
        return Result.Success();
    }

    public async Task<Result<UserDto>> GetByIdAsync(int userId)
    {
        var user = await _repository.GetByIdAsync(userId);
        if (user is null)
        {
            return Result.Failure<UserDto>(Error.NotFound("Пользователь не найден"));
        }

        return Result.Success(new UserDto(user.Id, user.Role, user.Email, user.PasswordHash, user.EmailVerified, user.FirstName,
            user.LastName, user.Patronymic, user.Phone));
    }

    public async Task<Result<UserDto>> GetByEmailAsync(string email)
    {
        var user = await _repository.GetByEmailAsync(email);
        if (user is null)
        {
            return Result.Failure<UserDto>(Error.NotFound("Пользователь не найден"));
        }
        
        return Result.Success(new UserDto(user.Id, user.Role, user.Email, user.PasswordHash, user.EmailVerified, user.FirstName,
            user.LastName, user.Patronymic, user.Phone));
    }
    
    public async Task<Result> UpdateEmailRequestAsync(int userId, string newEmail)
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

    public async Task<Result<PaginatedList<UserDto>>> GetPaginatedUsersAsync(UserFilters filters, int pageNumber, int pageSize)
    {
        var query = _repository.Query();

        if (!string.IsNullOrWhiteSpace(filters.FirstName))
        {
            query = query.Where(u => u.FirstName.Contains(filters.FirstName));
        }

        if (!string.IsNullOrWhiteSpace(filters.LastName))
        {
            query = query.Where(u => u.LastName.Contains(filters.LastName));
        }

        if (!string.IsNullOrWhiteSpace(filters.Patronymic))
        {
            query = query.Where(u => u.Patronymic.Contains(filters.Patronymic));
        }

        if (!string.IsNullOrWhiteSpace(filters.Email))
        {
            query = query.Where(u => u.Email.Contains(filters.Email));
        }

        if (filters.Role.HasValue)
        {
            query = query.Where(u => u.Role == filters.Role.Value);
        }

        var totalCount = await query.CountAsync();

        var users = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new UserDto(
                u.Id, 
                u.Role, 
                u.Email, 
                null, 
                u.EmailVerified, 
                u.FirstName, 
                u.LastName,
                u.Patronymic, 
                u.Phone))
            .ToListAsync();

        return Result.Success(new PaginatedList<UserDto>(users, totalCount, pageNumber, pageSize));
    }
}