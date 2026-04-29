using ERP.Shared.Domain;

namespace ERP.Modules.Wms.Domain;

public enum WarehouseOperationType
{
    Receiving = 1,
    Putaway = 2,
    Storage = 3,
    Picking = 4,
    Dispatch = 5
}

public enum WarehouseOperationStatus
{
    Pending = 1,
    InProgress = 2,
    Completed = 3,
    Cancelled = 4
}

public sealed class WarehouseOperation : CompanyEntity
{
    public long WarehouseId { get; private set; }
    public WarehouseOperationType OperationType { get; private set; }
    public WarehouseOperationStatus Status { get; private set; }
    public long? SourceLocationId { get; private set; }
    public long? DestinationLocationId { get; private set; }
    public string ReferenceType { get; private set; } = null!;
    public string ReferenceId { get; private set; } = null!;
    public long? AssignedToUserId { get; private set; }
    public DateTime? StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public string? Notes { get; private set; }

    private WarehouseOperation() { }

    public static WarehouseOperation Create(
        long companyId,
        long warehouseId,
        WarehouseOperationType operationType,
        string referenceType,
        string referenceId,
        long? sourceLocationId = null,
        long? destinationLocationId = null,
        long? assignedToUserId = null)
    {
        if (string.IsNullOrWhiteSpace(referenceType))
        {
            throw new ArgumentException("ReferenceType is required.", nameof(referenceType));
        }

        if (string.IsNullOrWhiteSpace(referenceId))
        {
            throw new ArgumentException("ReferenceId is required.", nameof(referenceId));
        }

        return new WarehouseOperation
        {
            CompanyId = companyId,
            WarehouseId = warehouseId,
            OperationType = operationType,
            Status = WarehouseOperationStatus.Pending,
            SourceLocationId = sourceLocationId,
            DestinationLocationId = destinationLocationId,
            ReferenceType = referenceType.Trim(),
            ReferenceId = referenceId.Trim(),
            AssignedToUserId = assignedToUserId
        };
    }

    public void Assign(long assignedToUserId)
    {
        AssignedToUserId = assignedToUserId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Start()
    {
        if (Status != WarehouseOperationStatus.Pending)
        {
            throw new InvalidOperationException("Only pending operations can be started.");
        }

        Status = WarehouseOperationStatus.InProgress;
        StartedAt = DateTime.UtcNow;
        UpdatedAt = StartedAt;
    }

    public void Complete()
    {
        if (Status != WarehouseOperationStatus.InProgress)
        {
            throw new InvalidOperationException("Only in-progress operations can be completed.");
        }

        Status = WarehouseOperationStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        UpdatedAt = CompletedAt;
    }

    public void Cancel(string? reason = null)
    {
        if (Status == WarehouseOperationStatus.Completed)
        {
            throw new InvalidOperationException("Completed operations cannot be cancelled.");
        }

        Status = WarehouseOperationStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
        Notes = string.IsNullOrWhiteSpace(reason) ? Notes : reason.Trim();
    }
}
