using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AppDrugsV2.Domain.Entities;

namespace AppDrugsV2.Infrastructure.Persistence.Configurations
{
    public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
    {
        public void Configure(EntityTypeBuilder<Appointment> builder)
        {
            builder.ToTable("Appointments");

            builder.HasKey(a => a.Id);

            builder.Property(a => a.UserId)
                .IsRequired();

            builder.Property(a => a.GestorFarmaceuticoId)
                .IsRequired();

            builder.Property(a => a.Status)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(a => a.CreatedAt)
                .IsRequired();

            builder.Property(a => a.IsActive)
                .IsRequired();

            builder.Property(a => a.Observaciones)
                .HasMaxLength(500);

            builder.Property(a => a.ArchivoNombre)
                .HasMaxLength(200);

            builder.Property(a => a.ArchivoContentType)
                .HasMaxLength(100);

            // Relaciones
            builder.HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(a => a.GestorFarmaceutico)
                .WithMany()
                .HasForeignKey(a => a.GestorFarmaceuticoId)
                .OnDelete(DeleteBehavior.Restrict);

            // Índices
            builder.HasIndex(a => a.UserId);
            builder.HasIndex(a => a.GestorFarmaceuticoId);
            builder.HasIndex(a => a.Status);
            builder.HasIndex(a => a.CreatedAt);
        }
    }
}