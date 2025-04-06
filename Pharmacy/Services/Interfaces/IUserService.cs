using Pharmacy.Endpoints.Authorization;
using Pharmacy.Endpoints.User;
using Pharmacy.Models.Dtos;
using Pharmacy.Models.Result;

namespace Pharmacy.Services.Interfaces;

public interface IUserService
{
    Task<Result> RegisterAsync(RegisterRequest request);
    Task<Result> UpdateAsync(int id, UpdateUserRequest request);
    Task<Result<UserDto>> GetByIdAsync(int id);
    Task<bool> ExistsByEmailAsync(string email);
}