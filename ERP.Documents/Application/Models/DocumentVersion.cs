namespace ERP.Documents.Application.Models;

public sealed record DocumentVersion(
    string FileName,
    string ContentType,
    long SizeBytes,
    long CreatedByUserId,
    DateTimeOffset CreatedAtUtc,
    string? Notes)
{
    public static DocumentVersion Create(
        string fileName,
        string contentType,
        long sizeBytes,
        long createdByUserId,
        string? notes)
        => new(fileName, contentType, sizeBytes, createdByUserId, DateTimeOffset.UtcNow, notes);
}
