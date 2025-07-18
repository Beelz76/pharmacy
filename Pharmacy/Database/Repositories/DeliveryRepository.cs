﻿using Microsoft.EntityFrameworkCore;
using Pharmacy.Database.Entities;
using Pharmacy.Database.Repositories.Interfaces;

namespace Pharmacy.Database.Repositories;

public class DeliveryRepository : IDeliveryRepository
{
    private readonly PharmacyDbContext _context;

    public DeliveryRepository(PharmacyDbContext context)
    {
        _context = context;
    }

    public Task<Delivery?> GetByOrderIdAsync(int orderId)
    {
        return _context.Deliveries
            .Include(d => d.Order)
            .Include(d => d.UserAddress)
            .ThenInclude(ua => ua.Address)
            .FirstOrDefaultAsync(d => d.OrderId == orderId);
    }

    public async Task AddAsync(Delivery delivery)
    {
        _context.Deliveries.Add(delivery);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Delivery delivery)
    {
        _context.Deliveries.Update(delivery);
        await _context.SaveChangesAsync();
    }
    
    public Task<bool> AnyByAddressIdAsync(int userAddressId)
    {
        return _context.Deliveries.AnyAsync(d => d.UserAddressId == userAddressId);
    }

    public IQueryable<Delivery> QueryWithDetails()
    {
        return _context.Deliveries
            .Include(d => d.Order)
                .ThenInclude(o => o.Pharmacy)
                    .ThenInclude(p => p.Address)
            .Include(d => d.UserAddress)
                .ThenInclude(ua => ua.Address)
            .AsNoTracking();
    }
}