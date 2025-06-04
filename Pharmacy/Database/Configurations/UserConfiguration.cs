using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pharmacy.Database.Entities;
using Pharmacy.Shared.Enums;

namespace Pharmacy.Database.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        
        builder.HasKey(x => x.Id);

        builder.Property(x => x.LastName)
            .IsRequired()
            .HasMaxLength(50);
        
        builder.Property(x => x.FirstName)
            .IsRequired()
            .HasMaxLength(50);
        
        builder.Property(x => x.Patronymic)
            .HasMaxLength(50);

        builder.Property(x => x.Phone)
            .HasMaxLength(11);
        
        builder.Property(x => x.Email)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.PasswordHash)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.EmailVerified)
            .IsRequired();

        builder.Property(x => x.Role)
            .IsRequired()
            .HasConversion<string>();
        
        builder.Property(x => x.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(x => x.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
        
        builder.HasMany(x => x.Orders)
            .WithOne(x => x.User)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.FavoriteItems)
            .WithOne(x => x.User)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.CartItems)
            .WithOne(x => x.User)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.EmailVerificationCodes)
            .WithOne(x => x.User)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasMany(x => x.Addresses)
            .WithOne(x => x.User)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.Email).IsUnique();
        
        builder.HasData(new List<User>
        {
            new User
            {
                Id = 1,
                Email = "admin@example.com",
                EmailVerified = true,
                PasswordHash = "2C7C7E374A42F485DBB90E8C19C1B9CEA0BD13EF67816BB8F8A70FCD47F0B9AA-02286835EA819C04E58E380AD9C0A8AF",
                LastName = "test",
                FirstName = "test",
                Role = UserRoleEnum.Admin,
            },
            new User
            {
                Id = 2,
                Email = "employee@example.com",
                EmailVerified = true,
                PasswordHash = "2C7C7E374A42F485DBB90E8C19C1B9CEA0BD13EF67816BB8F8A70FCD47F0B9AA-02286835EA819C04E58E380AD9C0A8AF",
                LastName = "test",
                FirstName = "test",
                Role = UserRoleEnum.Employee,
            },
            new User
            {
                Id = 3,
                Email = "user@example.com",
                EmailVerified = true,
                PasswordHash = "2C7C7E374A42F485DBB90E8C19C1B9CEA0BD13EF67816BB8F8A70FCD47F0B9AA-02286835EA819C04E58E380AD9C0A8AF",
                LastName = "test",
                FirstName = "test",
                Role = UserRoleEnum.User
            }
        });
    }
}