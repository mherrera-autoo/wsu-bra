using ERP.Shared.Domain;

namespace ERP.Modules.Wms.Domain;

public sealed class WarehouseLocation : CompanyEntity
{
    public long WarehouseId { get; private set; }
    public string Aisle { get; private set; } = null!;
    public string Rack { get; private set; } = null!;
    public string Side { get; private set; } = null!;
    public int Level { get; private set; }
    public int Slot { get; private set; }
    public decimal Capacity { get; private set; }
    public bool IsPalletSlot { get; private set; }
    public bool IsStackable { get; private set; }
    public string? Notes { get; private set; }

    private WarehouseLocation() { }

    public static WarehouseLocation Create(
        long companyId,
        long warehouseId,
        string aisle,
        string rack,
        string side,
        int level,
        int slot,
        decimal capacity,
        bool isPalletSlot,
        bool isStackable,
        string? notes)
    {
        if (string.IsNullOrWhiteSpace(aisle)) throw new ArgumentException("Aisle is required.", nameof(aisle));
        if (string.IsNullOrWhiteSpace(rack)) throw new ArgumentException("Rack is required.", nameof(rack));
        if (string.IsNullOrWhiteSpace(side)) throw new ArgumentException("Side is required.", nameof(side));
        if (level <= 0) throw new ArgumentOutOfRangeException(nameof(level));
        if (slot <= 0) throw new ArgumentOutOfRangeException(nameof(slot));
        if (capacity <= 0) throw new ArgumentOutOfRangeException(nameof(capacity));
        if (isPalletSlot && isStackable)
        {
            throw new InvalidOperationException("Pallet slots cannot be stackable.");
        }

        return new WarehouseLocation
        {
            CompanyId = companyId,
            WarehouseId = warehouseId,
            Aisle = aisle.Trim(),
            Rack = rack.Trim(),
            Side = side.Trim(),
            Level = level,
            Slot = slot,
            Capacity = capacity,
            IsPalletSlot = isPalletSlot,
            IsStackable = isStackable,
            Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim()
        };
    }

    public void Update(
        string aisle,
        string rack,
        string side,
        int level,
        int slot,
        decimal capacity,
        bool isPalletSlot,
        bool isStackable,
        string? notes)
    {
        if (string.IsNullOrWhiteSpace(aisle)) throw new ArgumentException("Aisle is required.", nameof(aisle));
        if (string.IsNullOrWhiteSpace(rack)) throw new ArgumentException("Rack is required.", nameof(rack));
        if (string.IsNullOrWhiteSpace(side)) throw new ArgumentException("Side is required.", nameof(side));
        if (level <= 0) throw new ArgumentOutOfRangeException(nameof(level));
        if (slot <= 0) throw new ArgumentOutOfRangeException(nameof(slot));
        if (capacity <= 0) throw new ArgumentOutOfRangeException(nameof(capacity));
        if (isPalletSlot && isStackable)
        {
            throw new InvalidOperationException("Pallet slots cannot be stackable.");
        }

        Aisle = aisle.Trim();
        Rack = rack.Trim();
        Side = side.Trim();
        Level = level;
        Slot = slot;
        Capacity = capacity;
        IsPalletSlot = isPalletSlot;
        IsStackable = isStackable;
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
        UpdatedAt = DateTime.UtcNow;
    }
}
