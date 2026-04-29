namespace ERP.Modules.Rfid.Domain;

public sealed class RfidMovementLink : ERP.Shared.Domain.Entity
{
    public Guid CompanyPublicId { get; private set; }
    public string EventId { get; private set; } = null!;
    public string? SessionId { get; private set; }
    public string Epc { get; private set; } = null!;
    public Guid? ProductPublicId { get; private set; }
    public Guid? InventoryMovementPublicId { get; private set; }

    private RfidMovementLink() { }

    public static RfidMovementLink Create(Guid companyPublicId, string eventId, string? sessionId, string epc, Guid? productPublicId, Guid? inventoryMovementPublicId)
    {
        return new RfidMovementLink
        {
            CompanyPublicId = companyPublicId,
            EventId = eventId,
            SessionId = sessionId,
            Epc = epc,
            ProductPublicId = productPublicId,
            InventoryMovementPublicId = inventoryMovementPublicId
        };
    }
}
