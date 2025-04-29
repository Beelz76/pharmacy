namespace Pharmacy.Database.Entities;

public class PaymentStatus
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}