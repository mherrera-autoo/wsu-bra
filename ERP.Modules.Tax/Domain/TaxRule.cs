using ERP.Shared.Domain;

namespace ERP.Modules.Tax.Domain;

public sealed class TaxRule : CompanyEntity
{
    public long TaxGroupId { get; private set; }
    public long TaxId { get; private set; }
    public int Sequence { get; private set; }
    public decimal Rate { get; private set; }
    public bool IsCompound { get; private set; }

    private TaxRule() { }

    public static TaxRule Create(long companyId, long taxGroupId, long taxId, decimal rate, int sequence = 1, bool isCompound = false)
    {
        if (rate < 0) throw new ArgumentOutOfRangeException(nameof(rate));
        if (sequence <= 0) throw new ArgumentOutOfRangeException(nameof(sequence));
        return new TaxRule
        {
            CompanyId = companyId,
            TaxGroupId = taxGroupId,
            TaxId = taxId,
            Rate = rate,
            Sequence = sequence,
            IsCompound = isCompound
        };
    }
}
