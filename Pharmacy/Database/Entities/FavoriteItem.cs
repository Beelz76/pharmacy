namespace Pharmacy.Database.Entities;

public class FavoriteItem
{
    public int UserId { get; set; }
    public int ProductId { get; set; }

    public User User { get; set; } = default!;
    public Product Product { get; set; } = default!;
}