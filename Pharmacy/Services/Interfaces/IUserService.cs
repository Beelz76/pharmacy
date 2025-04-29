using Pharmacy.Endpoints.Users;
using Pharmacy.Shared.Dto;
using Pharmacy.Shared.Result;

namespace Pharmacy.Services.Interfaces;

public interface IUserService
{
    Task<Result<CreatedDto>> CreateAsync(CreateUserDto dto);
    Task<Result> UpdateProfileAsync(int userId, UpdateProfileRequest request);
    Task<Result<UserDto>> GetByIdAsync(int userId);
    Task<Result<UserDto>> GetByEmailAsync(string email);
    Task<Result> UpdateEmailRequestAsync(int userId, string newEmail);
    Task<Result> UpdatePasswordAsync(int userId, string currentPassword, string newPassword);
    Task<Result<PaginatedList<UserDto>>> GetPaginatedUsersAsync(UserFilters filters, int pageNumber, int pageSize);
}