using Pharmacy.Database.Entities;

namespace Pharmacy.Database.Repositories.Interfaces;

public interface IUserAddressRepository
{
    Task<List<UserAddress>> GetByUserIdAsync(int userId);
    Task<UserAddress?> GetByIdAsync(int userId, int addressId);
    Task AddAsync(UserAddress address);
    Task UpdateAsync(UserAddress address);
    Task DeleteAsync(UserAddress address);
    Task<Address> GetOrCreateAddressAsync(Address address);
}