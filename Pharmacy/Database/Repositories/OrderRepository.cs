using Microsoft.EntityFrameworkCore;
using Pharmacy.Database.Entities;
using Pharmacy.Database.Repositories.Interfaces;

namespace Pharmacy.Database.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly PharmacyDbContext _context;

    public OrderRepository(PharmacyDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Order order)
    {
        await _context.Orders.AddAsync(order);
        await _context.SaveChangesAsync();
    }
    
    public async Task UpdateAsync(Order order)
    {
        _context.Orders.Update(order);
        await _context.SaveChangesAsync();
    }

    public async Task<Order?> GetByIdWithDetailsAsync(int orderId, bool includeItems = true, bool includeStatus = true,
        bool includeProductImages = true, bool includePayment = false, bool includeUser = false,
        bool includePharmacy = false, bool includeDelivery = false)
    {
        var query = _context.Orders.AsQueryable();

        if (includeStatus)
            query = query.Include(o => o.Status);

        if (includeItems)
        {
            var itemQuery = query.Include(o => o.OrderItems).ThenInclude(oi => oi.Product);
            query = includeProductImages
                ? itemQuery.ThenInclude(p => p.Images)
                : itemQuery;
        }

        if (includePayment)
            query = query.Include(o => o.Payment);
        
        if (includeUser)
            query = query.Include(o => o.User);
        
        if (includePharmacy)
            query = query.Include(o => o.Pharmacy)
                .ThenInclude(p => p.Address);

        if (includeDelivery)
        {
            query = query.Include(o => o.Delivery)
                .ThenInclude(d => d.UserAddress)
                    .ThenInclude(ua => ua.Address);
        }
        
        return await query.FirstOrDefaultAsync(o => o.Id == orderId);
    }

    public async Task<IEnumerable<OrderStatus>> GetAllOrderStatuses()
    {
        return await _context.OrderStatuses.ToListAsync();
    }
    
    public IQueryable<Order> QueryWithStatus()
    {
        return _context.Orders
            .Include(o => o.Status)
            .Include(o => o.User)
            .AsNoTracking();
    }
}