namespace ERP.Documents.Domain;

public sealed class DocumentVersion
{
    public DocumentVersion(
        long id,
        Guid companyPublicId,
        string fileName,
        string contentType,
        long size,
        string? label,
        DateTime uploadedAt)
    {
        Id = id;
        CompanyPublicId = companyPublicId;
        FileName = fileName;
        ContentType = contentType;
        Size = size;
        Label = label;
        UploadedAt = uploadedAt;
    }

    public long Id { get; private set; }
    public Guid CompanyPublicId { get; private set; }
    public string FileName { get; }
    public string ContentType { get; }
    public long Size { get; }
    public string? Label { get; }
    public DateTime UploadedAt { get; }

    internal void AssignId(long id)
    {
        Id = id;
    }
}
