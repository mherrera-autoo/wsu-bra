using ERP.Modules.Tax.Application.Repositories;
using ERP.Modules.Tax.Domain;
using ERP.Shared.Application;
using TaxEntity = ERP.Modules.Tax.Domain.Tax;

namespace ERP.Modules.Tax.Application.Services;

public sealed class TaxService
{
    private readonly ITaxRepository _taxRepository;
    private readonly ITaxGroupRepository _taxGroupRepository;
    private readonly IUnitOfWork _unitOfWork;

    public TaxService(
        ITaxRepository taxRepository,
        ITaxGroupRepository taxGroupRepository,
        IUnitOfWork unitOfWork)
    {
        _taxRepository = taxRepository;
        _taxGroupRepository = taxGroupRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<TaxEntity>> CreateTaxAsync(long companyId, string code, string name, decimal rate, CancellationToken cancellationToken = default)
    {
        var tax = TaxEntity.Create(companyId, code, name, rate);
        await _taxRepository.AddAsync(tax, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<TaxEntity>.Ok(tax);
    }

    public async Task<Result<TaxGroup>> CreateTaxGroupAsync(long companyId, string name, CancellationToken cancellationToken = default)
    {
        var taxGroup = TaxGroup.Create(companyId, name);
        await _taxGroupRepository.AddAsync(taxGroup, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<TaxGroup>.Ok(taxGroup);
    }

    public async Task<Result<TaxGroup>> AddRuleAsync(
        long companyId,
        long taxGroupId,
        long taxId,
        decimal rate,
        int sequence = 1,
        CancellationToken cancellationToken = default)
    {
        var taxGroup = await _taxGroupRepository.GetByIdWithRulesAsync(taxGroupId, cancellationToken);
        if (taxGroup is null || taxGroup.CompanyId != companyId)
        {
            return Result<TaxGroup>.Fail("Tax group not found.");
        }

        var tax = await _taxRepository.GetByIdAsync(taxId, cancellationToken);
        if (tax is null || tax.CompanyId != companyId)
        {
            return Result<TaxGroup>.Fail("Tax not found.");
        }

        taxGroup.AddRule(taxId, rate, sequence);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<TaxGroup>.Ok(taxGroup);
    }
}
