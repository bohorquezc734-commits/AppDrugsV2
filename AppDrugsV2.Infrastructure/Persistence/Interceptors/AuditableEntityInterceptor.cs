using AppDrugsV2.Application.Common.Interfaces;
using AppDrugsV2.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Text.Json;

namespace AppDrugsV2.Infrastructure.Persistence.Interceptors
{
    public class AuditableEntityInterceptor : SaveChangesInterceptor
    {
        private readonly ICurrentUserService _currentUserService;

        public AuditableEntityInterceptor(ICurrentUserService currentUserService)
        {
            _currentUserService = currentUserService;
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            var context = eventData.Context;
            if (context == null) return base.SavingChangesAsync(eventData, result, cancellationToken);

            var userId = _currentUserService.UserId;

            var auditEntries = new List<AuditLog>();

            foreach (var entry in context.ChangeTracker.Entries())
            {
                if (entry.Entity is AuditLog || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                    continue;

                var auditEntry = new AuditLog
                {
                    UserId = userId,
                    EntityName = entry.Entity.GetType().Name,
                    Timestamp = DateTime.UtcNow,
                    Action = entry.State.ToString()
                };

                var primaryKey = entry.Properties
                    .Where(p => p.Metadata.IsPrimaryKey())
                    .ToDictionary(p => p.Metadata.Name, p => p.CurrentValue ?? p.OriginalValue);
                auditEntry.PrimaryKey = JsonSerializer.Serialize(primaryKey);

                var oldValues = new Dictionary<string, object?>();
                var newValues = new Dictionary<string, object?>();

                foreach (var property in entry.Properties)
                {
                    if (property.IsTemporary || property.Metadata.IsPrimaryKey())
                    {
                        continue;
                    }

                    string propertyName = property.Metadata.Name;

                    switch (entry.State)
                    {
                        case EntityState.Added:
                            newValues[propertyName] = property.CurrentValue;
                            break;
                        case EntityState.Deleted:
                            oldValues[propertyName] = property.OriginalValue;
                            break;
                        case EntityState.Modified:
                            if (property.IsModified)
                            {
                                oldValues[propertyName] = property.OriginalValue;
                                newValues[propertyName] = property.CurrentValue;
                            }
                            break;
                    }
                }

                if (oldValues.Any()) auditEntry.OldValues = JsonSerializer.Serialize(oldValues);
                if (newValues.Any()) auditEntry.NewValues = JsonSerializer.Serialize(newValues);

                auditEntries.Add(auditEntry);
            }

            if (auditEntries.Any())
            {
                context.Set<AuditLog>().AddRange(auditEntries);
            }

            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }
    }
}
