using Pharmacy.Database.Entities;

namespace Pharmacy.Database.Repositories.Interfaces;

public interface IAddressRepository
{
    Task<Address> GetOrCreateAddressAsync(Address address);
}