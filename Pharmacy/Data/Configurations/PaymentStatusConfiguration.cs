using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pharmacy.Extensions;
using Pharmacy.Models.Entities;
using Pharmacy.Models.Enums;

namespace Pharmacy.Data.Configurations;

public class PaymentStatusConfiguration: IEntityTypeConfiguration<PaymentStatus>
{
    public void Configure(EntityTypeBuilder<PaymentStatus> builder)
    {
        builder.ToTable("Ref_PaymentStatuses");
        
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(x => x.Description)
            .IsRequired()
            .HasColumnType("text");
        
        builder.HasMany(x => x.Payments)
            .WithOne(x => x.PaymentStatus)
            .HasForeignKey(x => x.PaymentMethodId);
        
        builder.HasData(EnumExtensions.GetEnumDetails<PaymentStatusEnum>()
            .Select(x => new PaymentStatus
            {
                Id = (int)x.Item1,
                Name = x.Item1.ToString(),
                Description = x.Item2
            }).ToArray());
    }
}