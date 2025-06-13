using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pharmacy.Database.Entities;

namespace Pharmacy.Database.Configurations;

public class DeliveryConfiguration : IEntityTypeConfiguration<Delivery>
{
    public void Configure(EntityTypeBuilder<Delivery> builder)
    {
        builder.ToTable("Deliveries");

        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Comment)
            .HasMaxLength(1000);
        
        builder.Property(x => x.Price)
            .IsRequired()
            .HasPrecision(18, 2);
        
        builder.Property(x => x.DeliveryDate);
        
        builder.HasOne(x => x.Order)
            .WithOne(x => x.Delivery)
            .HasForeignKey<Delivery>(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.UserAddress)
            .WithMany(x => x.Deliveries)
            .HasForeignKey(x => x.UserAddressId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}