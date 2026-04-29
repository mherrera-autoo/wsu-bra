using ERP.Modules.Pricing.Application.Repositories;
using ERP.Modules.Pricing.Domain;
using ERP.Shared.Application;

namespace ERP.Modules.Pricing.Application.Services;

public sealed class PricingPolicyService
{
    private readonly IPricingPolicyRepository _pricingPolicyRepository;
    private readonly IUnitOfWork _unitOfWork;

    public PricingPolicyService(
        IPricingPolicyRepository pricingPolicyRepository,
        IUnitOfWork unitOfWork)
    {
        _pricingPolicyRepository = pricingPolicyRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PricingPolicy>> GetAsync(long companyId, CancellationToken cancellationToken = default)
    {
        var policy = await _pricingPolicyRepository.GetAsync(companyId, cancellationToken);
        return policy is null
            ? Result<PricingPolicy>.Fail("Pricing policy not found.")
            : Result<PricingPolicy>.Ok(policy);
    }

    public async Task<Result<PricingPolicy>> UpsertAsync(
        long companyId,
        decimal marginPercent,
        PricingRoundingMode roundingMode,
        int? decimalPlaces,
        long updatedByUserId,
        CancellationToken cancellationToken = default)
    {
        var policy = await _pricingPolicyRepository.GetAsync(companyId, cancellationToken);
        if (policy is null)
        {
            policy = PricingPolicy.Create(companyId, marginPercent, roundingMode, decimalPlaces, updatedByUserId);
            await _pricingPolicyRepository.AddAsync(policy, cancellationToken);
        }
        else
        {
            policy.Update(marginPercent, roundingMode, decimalPlaces, updatedByUserId);
            await _pricingPolicyRepository.UpdateAsync(policy, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<PricingPolicy>.Ok(policy);
    }
}
