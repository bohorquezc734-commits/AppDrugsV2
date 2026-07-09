using Microsoft.EntityFrameworkCore;
using AppDrugsV2.Domain.Entities;

namespace AppDrugsV2.Application.Common.Interfaces
{
    public interface IApplicationDbContext
    {
        DbSet<User> Users { get; }
        DbSet<Drug> Drugs { get; }
        DbSet<GestorFarmaceutico> GestoresFarmaceuticos { get; } 
        DbSet<Inventory> Inventories { get; }
        DbSet<Appointment> Appointments { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}