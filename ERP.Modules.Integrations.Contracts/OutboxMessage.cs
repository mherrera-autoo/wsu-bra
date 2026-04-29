using ERP.Shared.Domain;
using System;

namespace ERP.Modules.Integrations.Contracts;

public sealed class OutboxMessage : Entity
{
    public string Type { get; private set; } = null!;
    public string PayloadJson { get; private set; } = null!;
    public int Attempts { get; private set; }
    public DateTime OccurredAt { get; private set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; private set; }
    public DateTime? NextRetryAt { get; private set; }
    public bool IsDeadLettered { get; private set; }
    public string? LastError { get; private set; }

    private OutboxMessage() { }

    public static OutboxMessage Create(string type, string payloadJson)
        => new() { Type = type.Trim(), PayloadJson = payloadJson };

    public void MarkFailed(string error, DateTime nextRetryAt)
    {
        Attempts++;
        LastError = error;
        NextRetryAt = nextRetryAt;
    }

    public void MarkDeadLettered(string error)
    {
        Attempts++;
        LastError = error;
        IsDeadLettered = true;
        NextRetryAt = null;
    }

    public void MarkProcessed()
    {
        ProcessedAt = DateTime.UtcNow;
        NextRetryAt = null;
    }
}
