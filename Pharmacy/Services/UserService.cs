using System.Net;
using Pharmacy.Database.Entities;
using Pharmacy.Database.Repositories.Interfaces;
using Pharmacy.DateTimeProvider;
using Pharmacy.Endpoints.Users;
using Pharmacy.Services.Interfaces;
using Pharmacy.Shared.Dto;
using Pharmacy.Shared.Enums;
using Pharmacy.Shared.Result;

namespace Pharmacy.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _repository;
    private readonly IDateTimeProvider _dateTimeProvider;
    public UserService(IUserRepository repository, IDateTimeProvider dateTimeProvider)
    {
        _repository = repository;
        _dateTimeProvider = dateTimeProvider;
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
}