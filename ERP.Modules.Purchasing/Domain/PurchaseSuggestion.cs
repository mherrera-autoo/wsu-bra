using ERP.Shared.Domain;

namespace ERP.Modules.Purchasing.Domain;

public enum PurchaseSuggestionStatus { Draft = 1, Approved = 2, Converted = 3, Rejected = 4 }

public enum PurchaseSuggestionReason { Stockout = 1, Minimum = 2, Manual = 3 }

public sealed class PurchaseSuggestion : CompanyEntity
{
    public PurchaseSuggestionStatus Status { get; private set; } = PurchaseSuggestionStatus.Draft;
    public long? SupplierSuggestedId { get; private set; }
    public long? ConvertedPurchaseOrderId { get; private set; }
    public List<PurchaseSuggestionLine> Lines { get; private set; } = new();

    private PurchaseSuggestion() { }

    public static PurchaseSuggestion Create(long companyId, long? supplierSuggestedId = null)
        => new()
        {
            CompanyId = companyId,
            SupplierSuggestedId = supplierSuggestedId
        };

    public void AddLine(long productId, decimal qtySuggested, long? supplierSuggestedId, PurchaseSuggestionReason reason, long? warehouseId)
    {
        if (Status != PurchaseSuggestionStatus.Draft)
        {
            throw new InvalidOperationException("Only Draft suggestions can be edited.");
        }

        Lines.Add(PurchaseSuggestionLine.Create(CompanyId, Id, productId, qtySuggested, supplierSuggestedId, reason, warehouseId));
    }

    public void ReplaceLines(IEnumerable<PurchaseSuggestionLineInput> lines)
    {
        if (Status != PurchaseSuggestionStatus.Draft)
        {
            throw new InvalidOperationException("Only Draft suggestions can be edited.");
        }

        Lines = lines.Select(line => PurchaseSuggestionLine.Create(
                CompanyId,
                Id,
                line.ProductId,
                line.QtySuggested,
                line.SupplierSuggestedId,
                line.Reason,
                line.WarehouseId))
            .ToList();
    }

    public void Approve()
    {
        if (Status != PurchaseSuggestionStatus.Draft)
        {
            throw new InvalidOperationException("Only Draft suggestions can be approved.");
        }

        Status = PurchaseSuggestionStatus.Approved;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Reject()
    {
        if (Status != PurchaseSuggestionStatus.Draft)
        {
            throw new InvalidOperationException("Only Draft suggestions can be rejected.");
        }

        Status = PurchaseSuggestionStatus.Rejected;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkConverted(long purchaseOrderId)
    {
        if (Status != PurchaseSuggestionStatus.Approved)
        {
            throw new InvalidOperationException("Only Approved suggestions can be converted.");
        }

        Status = PurchaseSuggestionStatus.Converted;
        ConvertedPurchaseOrderId = purchaseOrderId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateSupplierSuggestion(long? supplierSuggestedId)
    {
        if (Status != PurchaseSuggestionStatus.Draft)
        {
            throw new InvalidOperationException("Only Draft suggestions can be edited.");
        }

        SupplierSuggestedId = supplierSuggestedId;
        UpdatedAt = DateTime.UtcNow;
    }
}

public sealed class PurchaseSuggestionLine : CompanyEntity
{
    public long PurchaseSuggestionId { get; private set; }
    public long ProductId { get; private set; }
    public decimal QtySuggested { get; private set; }
    public long? SupplierSuggestedId { get; private set; }
    public PurchaseSuggestionReason Reason { get; private set; }
    public long? WarehouseId { get; private set; }

    private PurchaseSuggestionLine() { }

    public static PurchaseSuggestionLine Create(
        long companyId,
        long suggestionId,
        long productId,
        decimal qtySuggested,
        long? supplierSuggestedId,
        PurchaseSuggestionReason reason,
        long? warehouseId)
    {
        if (qtySuggested <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(qtySuggested));
        }

        return new PurchaseSuggestionLine
        {
            CompanyId = companyId,
            PurchaseSuggestionId = suggestionId,
            ProductId = productId,
            QtySuggested = qtySuggested,
            SupplierSuggestedId = supplierSuggestedId,
            Reason = reason,
            WarehouseId = warehouseId
        };
    }
}

public sealed record PurchaseSuggestionLineInput(
    long ProductId,
    decimal QtySuggested,
    long? SupplierSuggestedId,
    PurchaseSuggestionReason Reason,
    long? WarehouseId);
