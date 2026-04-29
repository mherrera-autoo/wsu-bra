using ERP.Shared.Domain;

namespace ERP.Modules.MasterData.Domain;

public sealed class Product : CompanyEntity
{
    public string Sku { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public string? Barcode { get; private set; }
    public bool IsStockable { get; private set; } = true;
    public bool IsSellable { get; private set; } = true;
    public bool IsPurchasable { get; private set; } = true;
    public long UnitOfMeasureId { get; private set; }
    public bool? IsStackable { get; private set; }
    public decimal? LengthCm { get; private set; }
    public decimal? WidthCm { get; private set; }
    public decimal? WeightKg { get; private set; }
    public string? StorageType { get; private set; }
    public UnitOfMeasure? UnitOfMeasure { get; private set; }
    public ICollection<ProductSupplier> ProductSuppliers { get; private set; } = new List<ProductSupplier>();

    private Product() { }

    public static Product Create(
        long companyId,
        string sku,
        string name,
        string? barcode,
        long unitOfMeasureId,
        bool isStockable,
        bool isSellable,
        bool isPurchasable,
        bool? isStackable = null,
        decimal? lengthCm = null,
        decimal? widthCm = null,
        decimal? weightKg = null,
        string? storageType = null)
    {
        if (isStockable && !isSellable && !isPurchasable)
        {
            throw new ArgumentException("Stockable products must be sellable or purchasable.");
        }

        ValidatePhysicalAttributes(lengthCm, widthCm, weightKg);

        return new Product
        {
            CompanyId = companyId,
            Sku = sku.Trim(),
            Name = name.Trim(),
            Barcode = string.IsNullOrWhiteSpace(barcode) ? null : barcode.Trim(),
            IsStockable = isStockable,
            IsSellable = isSellable,
            IsPurchasable = isPurchasable,
            UnitOfMeasureId = unitOfMeasureId,
            IsStackable = isStackable,
            LengthCm = lengthCm,
            WidthCm = widthCm,
            WeightKg = weightKg,
            StorageType = NormalizeNullable(storageType)
        };
    }

    public void Update(
        string sku,
        string name,
        bool isStockable,
        bool? isStackable = null,
        decimal? lengthCm = null,
        decimal? widthCm = null,
        decimal? weightKg = null,
        string? storageType = null)
    {
        if (isStockable && !IsSellable && !IsPurchasable)
        {
            throw new ArgumentException("Stockable products must be sellable or purchasable.");
        }

        ValidatePhysicalAttributes(lengthCm, widthCm, weightKg);

        Sku = sku.Trim();
        Name = name.Trim();
        IsStockable = isStockable;
        IsStackable = isStackable;
        LengthCm = lengthCm;
        WidthCm = widthCm;
        WeightKg = weightKg;
        StorageType = NormalizeNullable(storageType);
        UpdatedAt = DateTime.UtcNow;
    }

    private static string? NormalizeNullable(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static void ValidatePhysicalAttributes(decimal? lengthCm, decimal? widthCm, decimal? weightKg)
    {
        if (lengthCm.HasValue && lengthCm.Value < 0m) throw new ArgumentOutOfRangeException(nameof(lengthCm));
        if (widthCm.HasValue && widthCm.Value < 0m) throw new ArgumentOutOfRangeException(nameof(widthCm));
        if (weightKg.HasValue && weightKg.Value < 0m) throw new ArgumentOutOfRangeException(nameof(weightKg));
    }
}
