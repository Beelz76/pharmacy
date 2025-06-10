using Microsoft.EntityFrameworkCore;
using Pharmacy.Database.Entities;
using Pharmacy.Database.Repositories.Interfaces;

namespace Pharmacy.Database.Repositories;

public class PaymentMethodRepository : IPaymentMethodRepository
{
    private readonly PharmacyDbContext _context;

    public PaymentMethodRepository(PharmacyDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<PaymentMethod>> GetAllAsync()
    {
        return await _context.PaymentMethods
            .AsNoTracking()
            .ToListAsync();
    }
}