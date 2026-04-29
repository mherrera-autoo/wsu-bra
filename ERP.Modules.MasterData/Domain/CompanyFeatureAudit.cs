namespace ERP.Modules.MasterData.Domain;

public sealed class CompanyFeatureAudit
{
    public long Id { get; private set; }
    public long CompanyId { get; private set; }
    public Guid FeaturePublicId { get; private set; }
    public CompanyFeatureAuditAction Action { get; private set; }
    public DateTime ChangedAt { get; private set; }
    public long ChangedByUserId { get; private set; }
    public string? Reason { get; private set; }
    public Guid? CorrelationId { get; private set; }

    private CompanyFeatureAudit() { }

    public static CompanyFeatureAudit Create(
        long companyId,
        Guid featurePublicId,
        CompanyFeatureAuditAction action,
        DateTime changedAt,
        long changedByUserId,
        string? reason,
        Guid? correlationId)
    {
        if (featurePublicId == Guid.Empty)
        {
            throw new ArgumentException("FeaturePublicId is required.", nameof(featurePublicId));
        }

        return new CompanyFeatureAudit
        {
            CompanyId = companyId,
            FeaturePublicId = featurePublicId,
            Action = action,
            ChangedAt = changedAt,
            ChangedByUserId = changedByUserId,
            Reason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim(),
            CorrelationId = correlationId
        };
    }
}
