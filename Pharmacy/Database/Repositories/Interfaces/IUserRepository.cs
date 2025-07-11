﻿using Pharmacy.Database.Entities;

namespace Pharmacy.Database.Repositories.Interfaces;

public interface IUserRepository
{
    Task<IEnumerable<User>> GetAllAsync();
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByIdWithPharmacyAsync(int id);
    Task<User?> GetByEmailAsync(string email, int? excludeId = null);
    Task AddAsync(User user);
    Task UpdateAsync(User user);
    Task DeleteAsync(User user);
    IQueryable<User> Query();
}