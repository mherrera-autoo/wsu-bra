namespace ERP.Documents.Application.Models;

public sealed record DocumentLink(
    string LinkedEntityType,
    long LinkedEntityId,
    long CreatedByUserId,
    DateTimeOffset CreatedAtUtc)
{
    public static DocumentLink Create(string linkedEntityType, long linkedEntityId, long createdByUserId)
        => new(linkedEntityType, linkedEntityId, createdByUserId, DateTimeOffset.UtcNow);
}
