using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AppDrugsV2.Domain.Entities;

namespace AppDrugsV2.Infrastructure.Persistence.Configurations
{
    public class AppointmentDetailConfiguration : IEntityTypeConfiguration<AppointmentDetail>
    {
        public void Configure(EntityTypeBuilder<AppointmentDetail> builder)
        {
            builder.ToTable("AppointmentDetails");

            builder.HasKey(ad => ad.Id);

            builder.Property(ad => ad.AppointmentId)
                .IsRequired();

            builder.Property(ad => ad.InventoryId)
                .IsRequired();

            builder.Property(ad => ad.Quantity)
                .IsRequired();

            builder.Property(ad => ad.CreatedAt)
                .IsRequired();

            // Relaciones
            builder.HasOne(ad => ad.Appointment)
                .WithMany(a => a.Details)
                .HasForeignKey(ad => ad.AppointmentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(ad => ad.Inventory)
                .WithMany()
                .HasForeignKey(ad => ad.InventoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Índices
            builder.HasIndex(ad => ad.AppointmentId);
            builder.HasIndex(ad => ad.InventoryId);
        }
    }
}