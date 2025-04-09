using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pharmacy.Database.Entities;
using Pharmacy.Extensions;
using Pharmacy.Shared.Enums;

namespace Pharmacy.Database.Configurations;

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
            .WithOne(x => x.Status)
            .HasForeignKey(x => x.StatusId);
        
        builder.HasData(EnumExtensions.GetEnumDetails<PaymentStatusEnum>()
            .Select(x => new PaymentStatus
            {
                Id = (int)x.Item1,
                Name = x.Item1.ToString(),
                Description = x.Item2
            }).ToArray());
    }
}