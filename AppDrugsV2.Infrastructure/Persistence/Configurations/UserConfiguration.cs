using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AppDrugsV2.Domain.Entities;

namespace AppDrugsV2.Infrastructure.Persistence.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");

            builder.HasKey(u => u.Id);

            builder.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(u => u.PasswordHash)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(u => u.FullName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(u => u.Role)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(20);

            builder.Property(u => u.IsActive)
                .IsRequired();

            builder.Property(u => u.CreatedAt)
                .IsRequired();

            builder.HasIndex(u => u.Email)
                .IsUnique();
        }
    }
}