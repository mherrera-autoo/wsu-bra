using ERP.Modules.MasterData.Application.Repositories;
using ERP.Shared.Application;

namespace ERP.Modules.MasterData.Application.Services;

public interface ICompanyCurrencyValidationService
{
    Task<Result> ValidateCurrencyForCompanyAsync(long companyId, long currencyId, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<long>>> GetAllowedCurrencyIdsForCompanyAsync(long companyId, CancellationToken cancellationToken = default);
    Task<Result<long?>> GetDefaultCurrencyIdForCompanyAsync(long companyId, CancellationToken cancellationToken = default);
    Task<bool> IsCurrencyAllowedForCompanyAsync(long companyId, long currencyId, CancellationToken cancellationToken = default);
}

// TODO: Implement this service with proper repository pattern
// This service should be implemented in the Persistence layer and used to validate
// currency operations in PurchaseOrder, SalesDocument, etc.
// 
// Example usage in a service when creating/updating PurchaseOrder or SalesDocument:
// var validationResult = await _currencyValidationService.ValidateCurrencyForCompanyAsync(companyId, currencyId);
// if (!validationResult.Success)
// {
//     return Result.Fail(validationResult.Error);
// }
//
// The implementation should:
// 1. Check if CurrencyId is in CompanyCurrencies for the given CompanyId
// 2. Ensure the currency is active
// 3. Return appropriate validation results
// 4. Handle default currency validation when needed
public sealed class CompanyCurrencyValidationService : ICompanyCurrencyValidationService
{
    private readonly ICompanyCurrencyRepository _companyCurrencyRepository;

    public CompanyCurrencyValidationService(ICompanyCurrencyRepository companyCurrencyRepository)
    {
        _companyCurrencyRepository = companyCurrencyRepository;
    }

    public async Task<Result> ValidateCurrencyForCompanyAsync(long companyId, long currencyId, CancellationToken cancellationToken = default)
    {
        if (companyId <= 0)
        {
            return Result.Fail("CompanyId is required.");
        }

        if (currencyId <= 0)
        {
            return Result.Fail("CurrencyId is required.");
        }

        var companyCurrency = await _companyCurrencyRepository.GetAsync(companyId, currencyId, cancellationToken);
        if (companyCurrency is null)
        {
            return Result.Fail("Currency is not allowed for the company.");
        }

        if (!companyCurrency.IsActive)
        {
            return Result.Fail("Currency is disabled for the company.");
        }

        if (companyCurrency.Currency is not null && !companyCurrency.Currency.IsActive)
        {
            return Result.Fail("Currency is inactive.");
        }

        return Result.Ok();
    }

    public async Task<Result<IEnumerable<long>>> GetAllowedCurrencyIdsForCompanyAsync(long companyId, CancellationToken cancellationToken = default)
    {
        if (companyId <= 0)
        {
            return Result<IEnumerable<long>>.Fail("CompanyId is required.");
        }

        var currencies = await _companyCurrencyRepository.ListByCompanyAsync(companyId, activeOnly: true, cancellationToken);
        var allowedIds = currencies
            .Where(companyCurrency => companyCurrency.Currency is null || companyCurrency.Currency.IsActive)
            .Select(companyCurrency => companyCurrency.CurrencyId)
            .ToList();
        return Result<IEnumerable<long>>.Ok(allowedIds);
    }

    public async Task<Result<long?>> GetDefaultCurrencyIdForCompanyAsync(long companyId, CancellationToken cancellationToken = default)
    {
        if (companyId <= 0)
        {
            return Result<long?>.Fail("CompanyId is required.");
        }

        var defaultCurrency = await _companyCurrencyRepository.GetDefaultAsync(companyId, cancellationToken);
        if (defaultCurrency is null)
        {
            return Result<long?>.Fail("Company has no base currency configured.");
        }

        if (!defaultCurrency.IsActive)
        {
            return Result<long?>.Fail("Base currency is disabled for the company.");
        }

        if (defaultCurrency.Currency is not null && !defaultCurrency.Currency.IsActive)
        {
            return Result<long?>.Fail("Base currency is inactive.");
        }

        return Result<long?>.Ok(defaultCurrency.CurrencyId);
    }

    public async Task<bool> IsCurrencyAllowedForCompanyAsync(long companyId, long currencyId, CancellationToken cancellationToken = default)
    {
        var companyCurrency = await _companyCurrencyRepository.GetAsync(companyId, currencyId, cancellationToken);
        return companyCurrency is not null
            && companyCurrency.IsActive
            && (companyCurrency.Currency is null || companyCurrency.Currency.IsActive);
    }
}
