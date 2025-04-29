using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pharmacy.Database.Entities;
using Pharmacy.Extensions;
using Pharmacy.Shared.Enums;

namespace Pharmacy.Database.Configurations;

public class OrderStatusConfiguration : IEntityTypeConfiguration<OrderStatus>
{
    public void Configure(EntityTypeBuilder<OrderStatus> builder)
    {
        builder.ToTable("Ref_OrderStatuses");
        
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(x => x.Description)
            .IsRequired()
            .HasMaxLength(500);
        
        builder.HasMany(x => x.Orders)
            .WithOne(x => x.Status)
            .HasForeignKey(x => x.StatusId);
        
        builder.HasData(EnumExtensions.GetEnumDetails<OrderStatusEnum>()
            .Select(x => new OrderStatus
            {
                Id = (int)x.Item1,
                Name = x.Item1.ToString(),
                Description = x.Item2
            }).ToArray());
    }
}