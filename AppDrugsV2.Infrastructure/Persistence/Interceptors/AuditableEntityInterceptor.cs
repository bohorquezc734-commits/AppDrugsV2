using AppDrugsV2.Application.Common.Interfaces;
using AppDrugsV2.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace AppDrugsV2.Infrastructure.Persistence.Interceptors
{
    public class AuditableEntityInterceptor : SaveChangesInterceptor
    {
        private static readonly string[] _ignoredProperties = { "LastLoginAt", "RefreshToken", "RefreshTokenExpiryTime" };
        
        private readonly ICurrentUserService _currentUserService;
        private readonly List<AuditEntryWrapper> _pendingAuditEntries = new();

        public AuditableEntityInterceptor(ICurrentUserService currentUserService)
        {
            _currentUserService = currentUserService;
        }

        private class AuditEntryWrapper
        {
            public AuditLog AuditLog { get; set; } = null!;
            public List<PropertyEntry> PrimaryKeyProperties { get; set; } = new();
        }

        private bool ShouldMask(string propertyName)
        {
            var lower = propertyName.ToLower();
            return lower.Contains("base64") || lower.Contains("archivo") || lower.Contains("image") || lower.Contains("token") || lower.Contains("password");
        }

        private object? GetMaskedValue(string propertyName, object? originalValue)
        {
            if (originalValue == null) return null;
            if (ShouldMask(propertyName)) return "[Dato Oculto/Adjunto]";
            return originalValue;
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
            _pendingAuditEntries.Clear();

            foreach (var entry in context.ChangeTracker.Entries())
            {
                if (entry.Entity is AuditLog || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                    continue;

                if (entry.State == EntityState.Modified)
                {
                    bool hasSignificantChanges = entry.Properties.Any(p => 
                        p.IsModified && 
                        !_ignoredProperties.Contains(p.Metadata.Name));

                    if (!hasSignificantChanges)
                        continue;
                }

                var auditEntry = new AuditLog
                {
                    UserId = userId,
                    EntityName = entry.Entity.GetType().Name,
                    Timestamp = System.DateTime.UtcNow,
                    Action = entry.State.ToString()
                };

                var oldValues = new Dictionary<string, object?>();
                var newValues = new Dictionary<string, object?>();

                foreach (var property in entry.Properties)
                {
                    string propertyName = property.Metadata.Name;

                    if (property.IsTemporary || property.Metadata.IsPrimaryKey() || _ignoredProperties.Contains(propertyName))
                    {
                        continue;
                    }

                    switch (entry.State)
                    {
                        case EntityState.Added:
                            newValues[propertyName] = GetMaskedValue(propertyName, property.CurrentValue);
                            break;
                        case EntityState.Deleted:
                            oldValues[propertyName] = GetMaskedValue(propertyName, property.OriginalValue);
                            break;
                        case EntityState.Modified:
                            if (property.IsModified)
                            {
                                oldValues[propertyName] = GetMaskedValue(propertyName, property.OriginalValue);
                                newValues[propertyName] = GetMaskedValue(propertyName, property.CurrentValue);
                            }
                            break;
                    }
                }

                if (oldValues.Any()) auditEntry.OldValues = JsonSerializer.Serialize(oldValues);
                if (newValues.Any()) auditEntry.NewValues = JsonSerializer.Serialize(newValues);

                if (entry.State == EntityState.Added)
                {
                    var wrapper = new AuditEntryWrapper
                    {
                        AuditLog = auditEntry,
                        PrimaryKeyProperties = entry.Properties.Where(p => p.Metadata.IsPrimaryKey()).ToList()
                    };
                    _pendingAuditEntries.Add(wrapper);
                }
                else
                {
                    var primaryKey = entry.Properties
                        .Where(p => p.Metadata.IsPrimaryKey())
                        .ToDictionary(p => p.Metadata.Name, p => p.CurrentValue ?? p.OriginalValue);
                    auditEntry.PrimaryKey = JsonSerializer.Serialize(primaryKey);
                    
                    auditEntries.Add(auditEntry);
                }
            }

            if (auditEntries.Any())
            {
                context.Set<AuditLog>().AddRange(auditEntries);
            }

            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        public override async ValueTask<int> SavedChangesAsync(
            SaveChangesCompletedEventData eventData,
            int result,
            CancellationToken cancellationToken = default)
        {
            var context = eventData.Context;
            if (context == null || !_pendingAuditEntries.Any())
                return await base.SavedChangesAsync(eventData, result, cancellationToken);

            var newAuditEntries = new List<AuditLog>();

            foreach (var pending in _pendingAuditEntries)
            {
                var primaryKey = pending.PrimaryKeyProperties
                    .ToDictionary(p => p.Metadata.Name, p => p.CurrentValue);

                pending.AuditLog.PrimaryKey = JsonSerializer.Serialize(primaryKey);
                newAuditEntries.Add(pending.AuditLog);
            }

            _pendingAuditEntries.Clear();

            if (newAuditEntries.Any())
            {
                context.Set<AuditLog>().AddRange(newAuditEntries);
                await context.SaveChangesAsync(cancellationToken);
            }

            return await base.SavedChangesAsync(eventData, result, cancellationToken);
        }
    }
}
