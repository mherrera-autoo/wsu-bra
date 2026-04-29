namespace ERP.Modules.Rfid.Domain;

public sealed class OperatorCredential : ERP.Shared.Domain.Entity
{
    public Guid CompanyPublicId { get; private set; }
    public long OperatorId { get; private set; }
    public string? FaceTemplateId { get; private set; }
    public string? NfcCardUid { get; private set; }
    public bool IsActive { get; private set; }

    public Operator Operator { get; private set; } = null!;

    private OperatorCredential() { }

    public static OperatorCredential Create(Guid companyPublicId, long operatorId, string? faceTemplateId, string? nfcCardUid, bool isActive = true)
    {
        if (string.IsNullOrWhiteSpace(faceTemplateId) && string.IsNullOrWhiteSpace(nfcCardUid))
        {
            throw new InvalidOperationException("FaceTemplateId or NfcCardUid is required.");
        }

        return new OperatorCredential
        {
            CompanyPublicId = companyPublicId,
            OperatorId = operatorId,
            FaceTemplateId = string.IsNullOrWhiteSpace(faceTemplateId) ? null : faceTemplateId.Trim(),
            NfcCardUid = string.IsNullOrWhiteSpace(nfcCardUid) ? null : nfcCardUid.Trim(),
            IsActive = isActive,
            UpdatedAt = DateTime.UtcNow
        };
    }
}
