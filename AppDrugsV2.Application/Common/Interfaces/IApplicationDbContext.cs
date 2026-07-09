using AppDrugsV2.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AppDrugsV2.Application.Common.Interfaces
{
    public interface IApplicationDbContext
    {
        DbSet<User> Users { get; }
        DbSet<Drug> Drugs { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}