using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pharmacy.Extensions;
using Pharmacy.Models.Entities;
using Pharmacy.Models.Enums;

namespace Pharmacy.Data.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Ref_Roles");
        
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Description)
            .IsRequired()
            .HasColumnType("text");
        
        // builder.HasMany(x => x.Users)
        //     .WithOne(x => x.Role)
        //     .HasForeignKey(x => x.RoleId);
        
        builder.HasData(EnumExtensions.GetEnumDetails<UserRoleEnum>()
            .Select(x => new Role
            {
                Id = (int)x.Item1,
                Name = x.Item1.ToString(),
                Description = x.Item2
            }).ToArray());
    }
}