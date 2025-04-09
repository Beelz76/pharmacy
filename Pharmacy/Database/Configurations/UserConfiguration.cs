using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pharmacy.Database.Entities;

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
            .HasMaxLength(15);
        
        builder.Property(x => x.Email)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.PasswordHash)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.EmailVerified)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(x => x.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
        
        builder.HasMany(x => x.Orders)
            .WithOne(x => x.User)
            .HasForeignKey(x => x.UserId);

        builder.HasIndex(x => x.Email).IsUnique();
        
        builder.HasData(new List<User>
        {
            new User
            {
                Id = 1,
                Email = "test@gmail.com",
                EmailVerified = true,
                PasswordHash = "123456",
                LastName = "test",
                FirstName = "test"
            }
        });
    }
}