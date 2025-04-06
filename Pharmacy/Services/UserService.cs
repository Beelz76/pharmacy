using System.Net;
using Pharmacy.Data.Authorization;
using Pharmacy.Data.Repositories.Interfaces;
using Pharmacy.Endpoints.Authorization;
using Pharmacy.Endpoints.User;
using Pharmacy.Models.Dtos;
using Pharmacy.Models.Entities;
using Pharmacy.Models.Enums;
using Pharmacy.Models.Result;
using Pharmacy.Services.Interfaces;

namespace Pharmacy.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _repository;
    private readonly PasswordProvider _passwordProvider;
    public UserService(IUserRepository repository, PasswordProvider passwordProvider)
    {
        _repository = repository;
        _passwordProvider = passwordProvider;
    }

    public async Task<Result> RegisterAsync(RegisterRequest request)
    {
        var userExists = await _repository.ExistsByEmailAsync(request.Email);
        if (userExists)
        {
            return Result.Failure(HttpStatusCode.Conflict, ErrorTypeEnum.Conflict, "Пользователь с таким email уже зарегистрирован");
        }
        
        var user = new User
        {
            Email = request.Email,
            PasswordHash = _passwordProvider.Hash(request.Password),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Patronymic = request.Patronymic,
            Phone = request.Phone,
            EmailVerified = false
        };
        
        await _repository.AddAsync(user);
        return Result.Success();
    }

    public async Task<Result> UpdateAsync(int id, UpdateUserRequest request)
    {
        return await _repository.ExecuteInTransactionAsync(async () =>
        {
            var user = await _repository.GetByIdAsync(id);
            if (user is null)
            {
                return Result.Failure(HttpStatusCode.NotFound, ErrorTypeEnum.NotFound, "Производитель не найден");
            }

            if (user.Email != request.Email)
            {
                var userExists = await _repository.ExistsByEmailAsync(request.Email, id);
                if (userExists)
                {
                    return Result.Failure(HttpStatusCode.Conflict, ErrorTypeEnum.Conflict, "Пользователь с таким email уже зарегистрирован");
                }
                
                //TODO Выслать код на почту
            }

            user.Email = request.Email;
            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.Patronymic = request.Patronymic;
            user.Phone = request.Phone;

            await _repository.UpdateAsync(user);
            return Result.Success();
        });
    }

    public async Task<Result<UserDto>> GetByIdAsync(int id)
    {
        var user = await _repository.GetByIdAsync(id);
        if (user is null)
        {
            return Result<UserDto>.Failure(HttpStatusCode.NotFound, ErrorTypeEnum.NotFound, "Пользователь не найден");
        }

        return Result<UserDto>.Success(new UserDto(user.Id, user.Email, user.EmailVerified, user.FirstName,
            user.LastName, user.Patronymic, user.Phone));
    }
    
    public async Task<bool> ExistsByEmailAsync(string email)
    {
        return await _repository.ExistsByEmailAsync(email);
    }
}