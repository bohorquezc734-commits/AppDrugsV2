using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AppDrugsV2.Domain.Entities;

namespace AppDrugsV2.Infrastructure.Persistence.Configurations
{
    public class InventoryConfiguration : IEntityTypeConfiguration<Inventory>
    {
        public void Configure(EntityTypeBuilder<Inventory> builder)
        {
            builder.ToTable("Inventories");

            builder.HasKey(i => i.Id);

            builder.Property(i => i.DrugId)
                .IsRequired();

            builder.Property(i => i.GestorFarmaceuticoId)
                .IsRequired();

            builder.Property(i => i.Quantity)
                .IsRequired();

            builder.Property(i => i.IsActive)
                .IsRequired();

            builder.Property(i => i.CreatedAt)
                .IsRequired();

            // Relaciones
            builder.HasOne(i => i.Drug)
                .WithMany()
                .HasForeignKey(i => i.DrugId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(i => i.GestorFarmaceutico)
                .WithMany()
                .HasForeignKey(i => i.GestorFarmaceuticoId)
                .OnDelete(DeleteBehavior.Restrict);

            // Índice único: un medicamento solo puede tener un registro por sede
            builder.HasIndex(i => new { i.DrugId, i.GestorFarmaceuticoId })
                .IsUnique();
        }
    }
}