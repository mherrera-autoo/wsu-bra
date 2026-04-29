namespace ERP.Documents.Application.Models;

public sealed class Document
{
    private readonly List<DocumentLink> _links = new();
    private readonly List<DocumentVersion> _versions = new();

    private Document() { }

    public long Id { get; private set; }
    public Guid CompanyPublicId { get; private set; }
    public string FileName { get; private set; } = string.Empty;
    public string Category { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public DocumentStatus Status { get; private set; } = DocumentStatus.Active;
    public long? ArchivedByUserId { get; private set; }
    public DateTimeOffset? ArchivedAtUtc { get; private set; }
    public string? ArchiveReason { get; private set; }
    public long CreatedByUserId { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public IReadOnlyCollection<DocumentVersion> Versions => _versions;
    public IReadOnlyCollection<DocumentLink> Links => _links;

    public static Document Create(
        Guid companyPublicId,
        long createdByUserId,
        string fileName,
        string category,
        string contentType,
        long sizeBytes,
        string? description)
    {
        var document = new Document
        {
            CompanyPublicId = companyPublicId,
            CreatedByUserId = createdByUserId,
            FileName = fileName,
            Category = category,
            Description = description,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            Status = DocumentStatus.Active
        };

        document._versions.Add(DocumentVersion.Create(fileName, contentType, sizeBytes, createdByUserId, null));
        return document;
    }

    public void AddVersion(string fileName, string contentType, long sizeBytes, long createdByUserId, string? notes)
    {
        _versions.Add(DocumentVersion.Create(fileName, contentType, sizeBytes, createdByUserId, notes));
        FileName = fileName;
    }

    public void LinkTo(string linkedEntityType, long linkedEntityId, long createdByUserId)
    {
        _links.Add(DocumentLink.Create(linkedEntityType, linkedEntityId, createdByUserId));
    }

    public void Archive(string? reason, long archivedByUserId)
    {
        Status = DocumentStatus.Archived;
        ArchivedByUserId = archivedByUserId;
        ArchivedAtUtc = DateTimeOffset.UtcNow;
        ArchiveReason = string.IsNullOrWhiteSpace(reason) ? ArchiveReason : reason.Trim();
    }
}
