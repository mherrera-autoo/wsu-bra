using ERP.Modules.MasterData.Application.Repositories;
using ERP.Modules.MasterData.Domain;
using ERP.Shared.Application;

namespace ERP.Modules.MasterData.Application.Services;

public sealed class CompanyCurrencyService
{
    private readonly ICompanyCurrencyRepository _companyCurrencyRepository;
    private readonly ICurrencyRepository _currencyRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CompanyCurrencyService(
        ICompanyCurrencyRepository companyCurrencyRepository,
        ICurrencyRepository currencyRepository,
        IUnitOfWork unitOfWork)
    {
        _companyCurrencyRepository = companyCurrencyRepository;
        _currencyRepository = currencyRepository;
        _unitOfWork = unitOfWork;
    }

    public Task<IReadOnlyList<CompanyCurrency>> ListAsync(long companyId, bool activeOnly, CancellationToken cancellationToken = default)
        => _companyCurrencyRepository.ListByCompanyAsync(companyId, activeOnly, cancellationToken);

    public async Task<IReadOnlyList<CompanyCurrencyCatalogItem>> ListWithAssignmentAsync(
        long companyId,
        bool activeOnly,
        CancellationToken cancellationToken = default)
    {
        var currencies = await _currencyRepository.ListAsync(cancellationToken);
        var companyCurrencies = await _companyCurrencyRepository.ListByCompanyAsync(companyId, activeOnly: false, cancellationToken);
        var companyCurrencyById = companyCurrencies.ToDictionary(companyCurrency => companyCurrency.CurrencyId);

        var result = currencies
            .Select(currency =>
            {
                if (companyCurrencyById.TryGetValue(currency.Id, out var companyCurrency))
                {
                    return new CompanyCurrencyCatalogItem(
                        currency.Id,
                        currency.Code,
                        currency.Name,
                        currency.Order,
                        currency.Symbol,
                        IsAssigned: true,
                        companyCurrency.IsDefault,
                        companyCurrency.IsActive);
                }

                return new CompanyCurrencyCatalogItem(
                    currency.Id,
                    currency.Code,
                    currency.Name,
                    currency.Order,
                    currency.Symbol,
                    IsAssigned: false,
                    IsDefault: false,
                    IsActive: false);
            });

        if (activeOnly)
        {
            result = result.Where(item => item.IsAssigned && item.IsActive);
        }

        return result.ToList();
    }

    public Task<CompanyCurrency?> GetAsync(long companyId, long currencyId, CancellationToken cancellationToken = default)
        => _companyCurrencyRepository.GetAsync(companyId, currencyId, cancellationToken);

    public async Task<Result<CompanyCurrency>> CreateAsync(
        long companyId,
        long currencyId,
        bool isDefault,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        var currency = await _currencyRepository.GetByIdAsync(currencyId, cancellationToken);
        if (currency is null)
        {
            return Result<CompanyCurrency>.Fail("Currency not found.");
        }

        if (await _companyCurrencyRepository.ExistsAsync(companyId, currencyId, cancellationToken))
        {
            return Result<CompanyCurrency>.Fail("Company currency already exists.");
        }

        var existingDefault = await _companyCurrencyRepository.GetDefaultAsync(companyId, cancellationToken);
        var shouldSetAsDefault = isDefault || existingDefault is null;
        var companyCurrency = CompanyCurrency.Create(companyId, currencyId, shouldSetAsDefault, isActive);

        if (shouldSetAsDefault && existingDefault is not null && existingDefault.CurrencyId != currencyId)
        {
            existingDefault.UnsetDefault();
        }

        await _companyCurrencyRepository.AddAsync(companyCurrency, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        companyCurrency = await _companyCurrencyRepository.GetAsync(companyId, currencyId, cancellationToken) ?? companyCurrency;
        return Result<CompanyCurrency>.Ok(companyCurrency);
    }

    public async Task<Result<CompanyCurrency>> UpdateAsync(
        long companyId,
        long currencyId,
        bool isDefault,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        var companyCurrency = await _companyCurrencyRepository.GetAsync(companyId, currencyId, cancellationToken);
        if (companyCurrency is null)
        {
            return Result<CompanyCurrency>.Fail("Company currency not found.");
        }

        if (!isActive && companyCurrency.IsDefault)
        {
            return Result<CompanyCurrency>.Fail("Default currency cannot be deactivated.");
        }

        if (isDefault)
        {
            var existingDefault = await _companyCurrencyRepository.GetDefaultAsync(companyId, cancellationToken);
            if (existingDefault is not null && existingDefault.CurrencyId != currencyId)
            {
                existingDefault.UnsetDefault();
            }

            if (!companyCurrency.IsDefault)
            {
                companyCurrency.SetAsDefault();
            }
        }
        else if (companyCurrency.IsDefault)
        {
            return Result<CompanyCurrency>.Fail("At least one default currency is required.");
        }

        if (isActive && !companyCurrency.IsActive)
        {
            companyCurrency.Activate();
        }
        else if (!isActive && companyCurrency.IsActive)
        {
            companyCurrency.Deactivate();
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<CompanyCurrency>.Ok(companyCurrency);
    }

    public async Task<Result> DeleteAsync(long companyId, long currencyId, CancellationToken cancellationToken = default)
    {
        var companyCurrency = await _companyCurrencyRepository.GetAsync(companyId, currencyId, cancellationToken);
        if (companyCurrency is null)
        {
            return Result.Fail("Company currency not found.");
        }

        if (companyCurrency.IsDefault)
        {
            return Result.Fail("Default currency cannot be deleted.");
        }

        _companyCurrencyRepository.Remove(companyCurrency);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }
}

public sealed record CompanyCurrencyCatalogItem(
    long CurrencyId,
    string Code,
    string Name,
    int Order,
    string? Symbol,
    bool IsAssigned,
    bool IsDefault,
    bool IsActive);
