using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Pharmacy.Database.Entities;

namespace Pharmacy.Database;

public class PharmacyDbContext : DbContext
{
    public PharmacyDbContext(DbContextOptions<PharmacyDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Manufacturer> Manufacturers { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<OrderStatus> OrderStatuses { get; set; }
    public DbSet<ProductCategory> ProductCategories { get; set; }
    public DbSet<ProductCategoryField> ProductCategoryFields { get; set; }
    public DbSet<ProductProperty> ProductProperties { get; set; }
    public DbSet<ProductImage> ProductImages { get; set; }
    public DbSet<CartItem> CartItems { get; set; }
    public DbSet<FavoriteItem> FavoriteItems { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<PaymentStatus> PaymentStatus { get; set; }
    public DbSet<PaymentMethod> PaymentMethods { get; set; }
    public DbSet<EmailVerificationCode> EmailVerificationCodes { get; set; }
    public DbSet<Delivery> Deliveries { get; set; }
    public DbSet<Entities.Pharmacy> Pharmacies { get; set; }
    public DbSet<PharmacyProduct> PharmacyProducts { get; set; }
    public DbSet<UserAddress> UserAddresses { get; set; }
    public DbSet<Address> Addresses { get; set; }
    
    //public DbSet<RefreshToken> RefreshTokens { get; set; }
    
    protected override void OnModelCreating(ModelBuilder builder) 
    {
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(builder);
    }
}