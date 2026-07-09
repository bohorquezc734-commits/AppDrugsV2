using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AppDrugsV2.Domain.Entities;

namespace AppDrugsV2.Infrastructure.Persistence.Configurations
{
    public class GestorFarmaceuticoConfiguration : IEntityTypeConfiguration<GestorFarmaceutico>
    {
        public void Configure(EntityTypeBuilder<GestorFarmaceutico> builder)
        {
            builder.ToTable("GestoresFarmaceuticos");

            builder.HasKey(g => g.Id);

            builder.Property(g => g.NombreSede)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(g => g.Direccion)
                .IsRequired()
                .HasMaxLength(300);

            builder.Property(g => g.Telefono)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(g => g.IdEps)
                .IsRequired();

            builder.Property(g => g.IsActive)
                .IsRequired();

            builder.Property(g => g.CreatedAt)
                .IsRequired();

            builder.HasIndex(g => g.NombreSede)
                .IsUnique(); // No pueden existir dos sedes con el mismo nombre
        }
    }
}