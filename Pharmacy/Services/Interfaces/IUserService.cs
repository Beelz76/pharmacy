using Pharmacy.Endpoints.Users;
using Pharmacy.Shared.Dto;
using Pharmacy.Shared.Result;

namespace Pharmacy.Services.Interfaces;

public interface IUserService
{
    Task<Result<int>> CreateAsync(CreateUserDto dto);
    Task<Result> UpdateAsync(int id, UpdateUserRequest request);
    Task<Result<UserDto>> GetByIdAsync(int id);
    Task<Result<UserDto>> GetByEmailAsync(string email);
}