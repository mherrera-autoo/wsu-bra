namespace ERP.Modules.Rfid.Domain;

public sealed class RfidTag : ERP.Shared.Domain.Entity
{
    public Guid CompanyPublicId { get; private set; }
    public Guid? WarehousePublicId { get; private set; }
    public string Epc { get; private set; } = null!;
    public RfidTagStatus Status { get; private set; }
    public string? Notes { get; private set; }

    private RfidTag() { }

    public static RfidTag Create(Guid companyPublicId, Guid? warehousePublicId, string epc, string? notes)
    {
        if (string.IsNullOrWhiteSpace(epc))
        {
            throw new InvalidOperationException("EPC is required.");
        }

        return new RfidTag
        {
            CompanyPublicId = companyPublicId,
            WarehousePublicId = warehousePublicId,
            Epc = epc.Trim(),
            Status = RfidTagStatus.Active,
            Notes = notes
        };
    }

    public void Update(RfidTagStatus status, string? notes)
    {
        Status = status;
        Notes = notes;
        UpdatedAt = DateTime.UtcNow;
    }
}
