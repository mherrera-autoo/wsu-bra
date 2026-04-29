using ERP.Shared.Domain;

namespace ERP.Modules.Wms.Domain;

public sealed class DispatchConfirmation : CompanyEntity
{
    public string ReferenceType { get; private set; } = null!;
    public string ReferenceId { get; private set; } = null!;
    public DateTime DeliveredAt { get; private set; }
    public string Carrier { get; private set; } = null!;
    public string? DriverName { get; private set; }
    public string? VehiclePlate { get; private set; }
    public string? ReceivedBy { get; private set; }
    public string? Notes { get; private set; }
    public long ConfirmedByUserId { get; private set; }

    private DispatchConfirmation() { }

    public static DispatchConfirmation Create(
        long companyId,
        string referenceType,
        string referenceId,
        DateTime deliveredAt,
        string carrier,
        string? driverName,
        string? vehiclePlate,
        string? receivedBy,
        string? notes,
        long confirmedByUserId)
    {
        if (string.IsNullOrWhiteSpace(referenceType))
        {
            throw new ArgumentException("ReferenceType is required.", nameof(referenceType));
        }

        if (string.IsNullOrWhiteSpace(referenceId))
        {
            throw new ArgumentException("ReferenceId is required.", nameof(referenceId));
        }

        if (string.IsNullOrWhiteSpace(carrier))
        {
            throw new ArgumentException("Carrier is required.", nameof(carrier));
        }

        return new DispatchConfirmation
        {
            CompanyId = companyId,
            ReferenceType = referenceType.Trim(),
            ReferenceId = referenceId.Trim(),
            DeliveredAt = deliveredAt,
            Carrier = carrier.Trim(),
            DriverName = string.IsNullOrWhiteSpace(driverName) ? null : driverName.Trim(),
            VehiclePlate = string.IsNullOrWhiteSpace(vehiclePlate) ? null : vehiclePlate.Trim(),
            ReceivedBy = string.IsNullOrWhiteSpace(receivedBy) ? null : receivedBy.Trim(),
            Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim(),
            ConfirmedByUserId = confirmedByUserId
        };
    }
}
