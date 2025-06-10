using Microsoft.EntityFrameworkCore;
using Pharmacy.Database.Entities;
using Pharmacy.Database.Repositories.Interfaces;

namespace Pharmacy.Database.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly PharmacyDbContext _context;

    public PaymentRepository(PharmacyDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Payment payment)
    {
        await _context.Payments.AddAsync(payment);
        await _context.SaveChangesAsync();
    }
    
    public async Task UpdateAsync(Payment payment)
    {
        _context.Payments.Update(payment);
        await _context.SaveChangesAsync();
    }
    
    public async Task<Payment?> GetByOrderIdAsync(int orderId)
    {
        return await _context.Payments.FirstOrDefaultAsync(p => p.OrderId == orderId);
    }
    
    public async Task<Payment?> GetByIdWithDetailsAsync(int id)
    {
        return await _context.Payments
            .Include(p => p.Order)
            .ThenInclude(o => o.Pharmacy)
            .ThenInclude(ph => ph.Address)
            .Include(p => p.Status)
            .Include(p => p.Method)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public IQueryable<Payment> QueryWithDetails()
    {
        return _context.Payments
            .Include(p => p.Order)
            .ThenInclude(o => o.Pharmacy)
            .ThenInclude(ph => ph.Address)
            .Include(p => p.Status)
            .Include(p => p.Method)
            .AsNoTracking();
    }
}