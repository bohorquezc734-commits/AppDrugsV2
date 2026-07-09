using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AppDrugsV2.Domain.Entities;

namespace AppDrugsV2.Infrastructure.Persistence.Configurations
{
    public class DrugConfiguration : IEntityTypeConfiguration<Drug>
    {
        public void Configure(EntityTypeBuilder<Drug> builder)
        {
            builder.ToTable("Drugs");

            builder.HasKey(d => d.Id);

            builder.Property(d => d.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(d => d.GenericName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(d => d.Laboratory)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(d => d.Price)
                .IsRequired()
                .HasPrecision(18, 2);

            builder.Property(d => d.Stock)
                .IsRequired();

            builder.Property(d => d.Category)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(d => d.RequiresPrescription)
                .IsRequired();

            builder.Property(d => d.ExpirationDate)
                .IsRequired();

            builder.Property(d => d.IsActive)
                .IsRequired();

            builder.Property(d => d.CreatedAt)
                .IsRequired();

            builder.HasIndex(d => d.Name);
            builder.HasIndex(d => d.Category);
        }
    }
}