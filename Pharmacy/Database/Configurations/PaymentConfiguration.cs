using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pharmacy.Database.Entities;

namespace Pharmacy.Database.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");
        
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.ExternalPaymentId)
            .HasMaxLength(100);
        
        builder.Property(x => x.Number)
            .IsRequired()
            .HasMaxLength(20);
        
        builder.Property(x => x.Amount)
            .IsRequired()
            .HasPrecision(18, 2);
        
        builder.Property(x => x.Comment);
        
        builder.Property(x => x.TransactionDate);

        builder.Property(p => p.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(p => p.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
            
        builder.HasOne(x => x.Order)
            .WithOne(x => x.Payment)
            .HasForeignKey<Payment>(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(x => x.Method)
            .WithMany(x => x.Payments)
            .HasForeignKey(x => x.PaymentMethodId)
            .OnDelete(DeleteBehavior.Restrict);

            
        builder.HasOne(x => x.Status)
            .WithMany(x => x.Payments)
            .HasForeignKey(x => x.StatusId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasIndex(x => x.Number).IsUnique();
    }
}