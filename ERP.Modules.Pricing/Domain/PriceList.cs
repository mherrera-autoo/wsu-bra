using ERP.Shared.Domain;
using ERP.Shared.Domain.ValueObjects;

namespace ERP.Modules.Pricing.Domain;

public sealed class PriceList : CompanyEntity
{
    public string Name { get; private set; } = null!;
    public bool IsDefault { get; private set; }

    private PriceList() { }

    public static PriceList Create(long companyId, string name, bool isDefault = false)
        => new() { CompanyId = companyId, Name = name.Trim(), IsDefault = isDefault };
}

public sealed class PriceListItem : CompanyEntity
{
    public long PriceListId { get; private set; }
    public long ProductId { get; private set; }
    public Money UnitPrice { get; private set; }

    private PriceListItem() { }

    public static PriceListItem Create(long companyId, long priceListId, long productId, Money unitPrice)
        => new() { CompanyId = companyId, PriceListId = priceListId, ProductId = productId, UnitPrice = unitPrice };

    public void UpdatePrice(Money unitPrice)
    {
        UnitPrice = unitPrice;
    }
}
