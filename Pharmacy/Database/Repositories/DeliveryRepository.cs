using Microsoft.EntityFrameworkCore;
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
            .Include(x => x.UserAddress)
                .ThenInclude(x => x.Address)
            .FirstOrDefaultAsync(x => x.OrderId == orderId);
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
}