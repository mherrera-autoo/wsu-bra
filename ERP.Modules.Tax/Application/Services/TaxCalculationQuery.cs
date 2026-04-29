using ERP.Modules.Tax.Application.Repositories;
using ERP.Modules.Tax.Contracts;
using ERP.Shared.Application;
using ERP.Shared.Domain.ValueObjects;

namespace ERP.Modules.Tax.Application.Services;

public sealed class TaxCalculationQuery : ITaxCalculationQuery
{
    private readonly ITaxGroupRepository _taxGroupRepository;
    private readonly TaxCalculator _taxCalculator;

    public TaxCalculationQuery(ITaxGroupRepository taxGroupRepository, TaxCalculator taxCalculator)
    {
        _taxGroupRepository = taxGroupRepository;
        _taxCalculator = taxCalculator;
    }

    public async Task<Result<Money>> CalculateTaxAmountAsync(
        long companyId,
        long? taxGroupId,
        decimal baseAmount,
        string currency,
        CancellationToken cancellationToken = default)
    {
        if (taxGroupId is null)
        {
            return Result<Money>.Ok(new Money(0m, currency));
        }

        var taxGroup = await _taxGroupRepository.GetByIdWithRulesAsync(taxGroupId.Value, cancellationToken);
        if (taxGroup is null || taxGroup.CompanyId != companyId)
        {
            return Result<Money>.Fail("Tax group not found.");
        }

        var calculation = _taxCalculator.CalculateLineTaxes(new Money(baseAmount, currency), taxGroup);
        return Result<Money>.Ok(calculation.TaxAmount);
    }
}
