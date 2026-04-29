using ERP.Modules.Tax.Domain;
using ERP.Shared.Domain.ValueObjects;

namespace ERP.Modules.Tax.Application.Services;

public sealed record TaxCalculationResult(Money TaxAmount, Money TotalAmount);

public sealed class TaxCalculator
{
    public TaxCalculationResult CalculateLineTaxes(Money baseAmount, TaxGroup? taxGroup)
    {
        if (taxGroup is null || taxGroup.Rules.Count == 0)
        {
            return new TaxCalculationResult(new Money(0m, baseAmount.Currency), baseAmount);
        }

        decimal totalTax = 0m;
        var orderedRules = taxGroup.Rules.OrderBy(rule => rule.Sequence);
        foreach (var rule in orderedRules)
        {
            var taxableBase = rule.IsCompound ? baseAmount.Amount + totalTax : baseAmount.Amount;
            var lineTax = Math.Round(taxableBase * rule.Rate, 2, MidpointRounding.AwayFromZero);
            totalTax += lineTax;
        }

        var taxAmount = new Money(totalTax, baseAmount.Currency);
        var totalAmount = new Money(baseAmount.Amount + totalTax, baseAmount.Currency);
        return new TaxCalculationResult(taxAmount, totalAmount);
    }
}
