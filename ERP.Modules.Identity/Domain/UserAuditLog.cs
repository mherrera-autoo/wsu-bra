using ERP.Shared.Domain;

namespace ERP.Modules.Identity.Domain;

public sealed class UserAuditLog : Entity
{
    private UserAuditLog()
    {
    }

    private UserAuditLog(string? userId, string action, string entityName, string entityId, string changesJson, string source, DateTime occurredAt)
    {
        UserId = userId;
        Action = action;
        EntityName = entityName;
        EntityId = entityId;
        ChangesJson = changesJson;
        Source = source;
        OccurredAt = occurredAt;
    }

    public string? UserId { get; private set; }
    public string Action { get; private set; } = string.Empty;
    public string EntityName { get; private set; } = string.Empty;
    public string EntityId { get; private set; } = string.Empty;
    public string ChangesJson { get; private set; } = "{}";
    public string Source { get; private set; } = string.Empty;
    public DateTime OccurredAt { get; private set; }

    public static UserAuditLog Create(
        string? userId,
        string action,
        string entityName,
        string entityId,
        string changesJson,
        string source,
        DateTime occurredAt)
        => new(userId, action, entityName, entityId, changesJson, source, occurredAt);
}
