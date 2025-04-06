namespace Pharmacy.Models.Entities;

public class PaymentStatus
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}