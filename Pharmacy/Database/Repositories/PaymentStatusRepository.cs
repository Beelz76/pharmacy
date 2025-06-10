using Microsoft.EntityFrameworkCore;
using Pharmacy.Database.Entities;
using Pharmacy.Database.Repositories.Interfaces;

namespace Pharmacy.Database.Repositories;

public class PaymentStatusRepository : IPaymentStatusRepository
{
    private readonly PharmacyDbContext _context;

    public PaymentStatusRepository(PharmacyDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<PaymentStatus>> GetAllAsync()
    {
        return await _context.PaymentStatus
            .AsNoTracking()
            .ToListAsync();
    }
}