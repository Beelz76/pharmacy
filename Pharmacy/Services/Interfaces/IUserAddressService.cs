using Pharmacy.Endpoints.UserAddresses;
using Pharmacy.Shared.Dto;
using Pharmacy.Shared.Dto.User;
using Pharmacy.Shared.Result;

namespace Pharmacy.Services.Interfaces;

public interface IUserAddressService
{
    Task<Result<IEnumerable<UserAddressDto>>> GetAllAsync(int userId);
    Task<Result<UserAddressDto>> GetByIdAsync(int userId, int userAddressId);
    Task<Result<CreatedDto>> CreateAsync(int userId, CreateUserAddressRequest request);
    Task<Result> UpdateAsync(int userId, int userAddressId, UpdateUserAddressRequest request);
    Task<Result> DeleteAsync(int userId, int userAddressId);
}