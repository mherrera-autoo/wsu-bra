using ERP.Shared.Domain;

namespace ERP.Modules.MasterData.Domain;

public sealed class ProductSupplier : CompanyEntity
{
    public long ProductId { get; private set; }
    public long SupplierId { get; private set; }
    public string? SupplierSku { get; private set; }
    public decimal UnitPurchasePrice { get; private set; }
    public decimal MinimumPurchaseLot { get; private set; }

    public Product Product { get; private set; } = null!;
    public Supplier Supplier { get; private set; } = null!;

    private ProductSupplier() { }

    public static ProductSupplier Create(
        long companyId,
        long productId,
        long supplierId,
        string? supplierSku,
        decimal unitPurchasePrice,
        decimal minimumPurchaseLot)
    {
        if (unitPurchasePrice < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(unitPurchasePrice), "Unit purchase price cannot be negative.");
        }

        if (minimumPurchaseLot <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(minimumPurchaseLot), "Minimum purchase lot must be greater than zero.");
        }

        return new ProductSupplier
        {
            CompanyId = companyId,
            ProductId = productId,
            SupplierId = supplierId,
            SupplierSku = NormalizeSupplierSku(supplierSku),
            UnitPurchasePrice = unitPurchasePrice,
            MinimumPurchaseLot = minimumPurchaseLot
        };
    }

    public void UpdateCommercialTerms(string? supplierSku, decimal unitPurchasePrice, decimal minimumPurchaseLot)
    {
        if (unitPurchasePrice < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(unitPurchasePrice), "Unit purchase price cannot be negative.");
        }

        if (minimumPurchaseLot <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(minimumPurchaseLot), "Minimum purchase lot must be greater than zero.");
        }

        SupplierSku = NormalizeSupplierSku(supplierSku);
        UnitPurchasePrice = unitPurchasePrice;
        MinimumPurchaseLot = minimumPurchaseLot;
        UpdatedAt = DateTime.UtcNow;
    }

    private static string? NormalizeSupplierSku(string? supplierSku)
        => string.IsNullOrWhiteSpace(supplierSku) ? null : supplierSku.Trim();
}
