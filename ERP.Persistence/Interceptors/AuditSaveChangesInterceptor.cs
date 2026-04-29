using System.Text.Json;
using ERP.Modules.Identity.Domain;
using ERP.Shared.Application;
using ERP.Shared.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Runtime.CompilerServices;

namespace ERP.Persistence.Interceptors;

public sealed class AuditSaveChangesInterceptor : SaveChangesInterceptor
{
    private const string IdentitySchema = "iam";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private static readonly ConditionalWeakTable<DbContext, PendingAuditEntries> PendingAddedEntries = new();
    private static readonly AsyncLocal<bool> IsAuditSave = new();
    private readonly ICurrentUserProvider _currentUserProvider;

    public AuditSaveChangesInterceptor(ICurrentUserProvider currentUserProvider)
    {
        _currentUserProvider = currentUserProvider;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (IsAuditSave.Value)
        {
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        if (eventData.Context is null)
        {
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        var auditLogs = BuildAuditLogs(
            eventData.Context,
            out var pendingAddedEntries,
            out var pendingIdentityAddedEntries,
            out var identityAuditLogs);

        if (auditLogs.Count > 0)
        {
            eventData.Context.AddRange(auditLogs);
        }

        if (identityAuditLogs.Count > 0)
        {
            eventData.Context.AddRange(identityAuditLogs);
        }

        if (pendingAddedEntries.Count > 0 || pendingIdentityAddedEntries.Count > 0)
        {
            PendingAddedEntries.Remove(eventData.Context);
            PendingAddedEntries.Add(eventData.Context, new PendingAuditEntries(pendingAddedEntries, pendingIdentityAddedEntries));
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        if (IsAuditSave.Value || eventData.Context is null)
        {
            return await base.SavedChangesAsync(eventData, result, cancellationToken);
        }

        if (!PendingAddedEntries.TryGetValue(eventData.Context, out var pendingEntries) ||
            (pendingEntries.PublicAddedEntries.Count == 0 && pendingEntries.IdentityAddedEntries.Count == 0))
        {
            return await base.SavedChangesAsync(eventData, result, cancellationToken);
        }

        PendingAddedEntries.Remove(eventData.Context);

        var userId = _currentUserProvider.UserId?.ToString();
        var source = _currentUserProvider.Source ?? "system";
        var now = DateTime.UtcNow;

        var auditLogs = new List<AuditLog>(pendingEntries.PublicAddedEntries.Count);
        foreach (var entry in pendingEntries.PublicAddedEntries)
        {
            auditLogs.Add(AuditLog.Create(
                userId,
                "create",
                entry.Metadata.ClrType.Name,
                GetPrimaryKeyValue(entry),
                BuildChangesJson(entry, EntityState.Added),
                source,
                now));
        }

        var identityAuditLogs = new List<UserAuditLog>(pendingEntries.IdentityAddedEntries.Count);
        foreach (var entry in pendingEntries.IdentityAddedEntries)
        {
            identityAuditLogs.Add(UserAuditLog.Create(
                userId,
                "create",
                entry.Metadata.ClrType.Name,
                GetPrimaryKeyValue(entry),
                BuildChangesJson(entry, EntityState.Added),
                source,
                now));
        }

        if (auditLogs.Count > 0 || identityAuditLogs.Count > 0)
        {
            IsAuditSave.Value = true;
            try
            {
                if (auditLogs.Count > 0)
                {
                    eventData.Context.AddRange(auditLogs);
                }

                if (identityAuditLogs.Count > 0)
                {
                    eventData.Context.AddRange(identityAuditLogs);
                }

                await eventData.Context.SaveChangesAsync(cancellationToken);
            }
            finally
            {
                IsAuditSave.Value = false;
            }
        }

        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    public override Task SaveChangesFailedAsync(
        DbContextErrorEventData eventData,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            PendingAddedEntries.Remove(eventData.Context);
        }

        return base.SaveChangesFailedAsync(eventData, cancellationToken);
    }

    private List<AuditLog> BuildAuditLogs(
        DbContext dbContext,
        out List<EntityEntry> pendingAddedEntries,
        out List<EntityEntry> pendingIdentityAddedEntries,
        out List<UserAuditLog> identityAuditLogs)
    {
        var entries = dbContext.ChangeTracker
            .Entries()
            .Where(entry => entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .Where(entry => entry.Entity is not AuditLog and not UserAuditLog)
            .ToList();

        if (entries.Count == 0)
        {
            pendingAddedEntries = [];
            pendingIdentityAddedEntries = [];
            identityAuditLogs = [];
            return [];
        }

        var userId = _currentUserProvider.UserId?.ToString();
        var source = _currentUserProvider.Source ?? "system";
        var now = DateTime.UtcNow;

        var auditLogs = new List<AuditLog>(entries.Count);
        identityAuditLogs = new List<UserAuditLog>(entries.Count);
        pendingAddedEntries = [];
        pendingIdentityAddedEntries = [];
        foreach (var entry in entries)
        {
            var action = entry.State switch
            {
                EntityState.Added => "create",
                EntityState.Modified => "update",
                EntityState.Deleted => "delete",
                _ => "unknown"
            };

            var entityId = GetEntityIdentifier(entry);
            var changesJson = BuildChangesJson(entry, entry.State);

            if (IsIdentityAuditEntry(entry))
            {
                identityAuditLogs.Add(UserAuditLog.Create(
                    userId,
                    action,
                    entry.Metadata.ClrType.Name,
                    entityId,
                    changesJson,
                    source,
                    now));

                continue;
            }

            auditLogs.Add(AuditLog.Create(
                userId,
                action,
                entry.Metadata.ClrType.Name,
                entityId,
                changesJson,
                source,
                now));
        }

        return auditLogs;
    }

    private static bool IsIdentityAuditEntry(EntityEntry entry)
        => string.Equals(entry.Metadata.GetSchema(), IdentitySchema, StringComparison.OrdinalIgnoreCase);

    private static string GetEntityIdentifier(EntityEntry entry)
    {
        var publicId = entry.Properties
            .FirstOrDefault(property => property.Metadata.Name == "PublicId");

        if (publicId?.CurrentValue is not null || publicId?.OriginalValue is not null)
        {
            return (publicId.CurrentValue ?? publicId.OriginalValue)?.ToString() ?? string.Empty;
        }

        return GetPrimaryKeyValue(entry);
    }

    private static string GetPrimaryKeyValue(EntityEntry entry)
    {
        var keys = entry.Properties
            .Where(property => property.Metadata.IsPrimaryKey())
            .Select(property => property.CurrentValue ?? property.OriginalValue)
            .Where(value => value is not null)
            .Select(value => value!.ToString());

        return string.Join("|", keys);
    }

    private static string BuildChangesJson(EntityEntry entry, EntityState state)
    {
        var changes = state switch
        {
            EntityState.Added => BuildCurrentValues(entry.CurrentValues),
            EntityState.Deleted => BuildCurrentValues(entry.OriginalValues),
            EntityState.Modified => BuildModifiedValues(entry),
            _ => new Dictionary<string, object?>()
        };

        return JsonSerializer.Serialize(changes, JsonOptions);
    }

    private static Dictionary<string, object?> BuildModifiedValues(EntityEntry entry)
    {
        var changes = new Dictionary<string, object?>();
        foreach (var property in entry.Properties)
        {
            if (!property.IsModified)
            {
                continue;
            }

            changes[property.Metadata.Name] = new
            {
                before = property.OriginalValue,
                after = property.CurrentValue
            };
        }

        return changes;
    }

    private static Dictionary<string, object?> BuildCurrentValues(PropertyValues values)
    {
        var changes = new Dictionary<string, object?>();
        foreach (var property in values.Properties)
        {
            changes[property.Name] = values[property];
        }

        return changes;
    }

    private sealed record PendingAuditEntries(
        List<EntityEntry> PublicAddedEntries,
        List<EntityEntry> IdentityAddedEntries);
}
