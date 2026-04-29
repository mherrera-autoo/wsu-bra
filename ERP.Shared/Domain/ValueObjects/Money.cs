using System;

namespace ERP.Shared.Domain.ValueObjects;

public sealed record class Money
{
    private static readonly HashSet<string> SupportedCurrencies = new(StringComparer.OrdinalIgnoreCase)
    {
        "CLP",
        "USD",
        "UF",
        "EUR"
    };

    public decimal Amount { get; }
    public string Currency { get; }

    public Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = NormalizeCurrency(currency);
    }

    public static Money CLP(decimal amount) => new(amount, "CLP");
    public override string ToString() => $"{Amount:0.##} {Currency}";

    private static string NormalizeCurrency(string currency)
    {
        if (string.IsNullOrWhiteSpace(currency))
        {
            throw new ArgumentException("Currency is required.", nameof(currency));
        }

        var normalized = currency.Trim().ToUpperInvariant();
        if (!SupportedCurrencies.Contains(normalized))
        {
            throw new ArgumentException($"Unsupported currency '{currency}'.", nameof(currency));
        }

        return normalized;
    }
}
