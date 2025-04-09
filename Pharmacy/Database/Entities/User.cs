namespace Pharmacy.Database.Entities;

public class User
{
    public int Id { get; set; }
    //public UserRoleEnum RoleId { get; set; }
    public string LastName { get; set; }
    public string FirstName { get; set; }
    public string? Patronymic { get; set; }
    public string Email { get; set; }
    public string? Phone { get; set; }
    public string PasswordHash { get; set; }
    public bool EmailVerified  { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    //public Role Role { get; set; } = default!;
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<FavoriteItem> FavoriteItems { get; set; } = new List<FavoriteItem>();
    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    public ICollection<EmailVerificationCode> EmailVerificationCodes { get; set; } = new List<EmailVerificationCode>();
}