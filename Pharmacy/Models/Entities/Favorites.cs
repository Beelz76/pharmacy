namespace Pharmacy.Models.Entities;

public class Favorites
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int ProductId { get; set; }

    public User User { get; set; } = default!;
    public Product Product { get; set; } = default!;
}