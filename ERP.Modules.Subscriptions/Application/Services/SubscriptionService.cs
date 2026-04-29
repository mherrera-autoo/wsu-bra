using ERP.Modules.Integrations.Contracts;
using ERP.Modules.MasterData.Application.Services;
using ERP.Modules.Subscriptions.Application.Models;
using ERP.Modules.Subscriptions.Application.Repositories;
using ERP.Modules.Subscriptions.Domain;
using ERP.Shared.Application;

namespace ERP.Modules.Subscriptions.Application.Services;

public sealed class SubscriptionService
{
    private readonly IPlanRepository _planRepository;
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly ICompanyFeatureManager _companyFeatureManager;
    private readonly IWebpayGateway _webpayGateway;
    private readonly IUnitOfWork _unitOfWork;

    public SubscriptionService(
        IPlanRepository planRepository,
        ISubscriptionRepository subscriptionRepository,
        ICompanyFeatureManager companyFeatureManager,
        IWebpayGateway webpayGateway,
        IUnitOfWork unitOfWork)
    {
        _planRepository = planRepository;
        _subscriptionRepository = subscriptionRepository;
        _companyFeatureManager = companyFeatureManager;
        _webpayGateway = webpayGateway;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<SubscriptionSummary>> OnboardCompanyAsync(
        long companyId,
        long planId,
        int? seatLimit,
        int? trialDays,
        long requestedByUserId,
        CancellationToken cancellationToken = default)
    {
        if (companyId <= 0)
        {
            return Result<SubscriptionSummary>.Fail("CompanyId is required.");
        }

        var plan = await _planRepository.GetByIdAsync(planId, cancellationToken);
        if (plan is null || !plan.IsActive)
        {
            return Result<SubscriptionSummary>.Fail("Plan not found or inactive.");
        }

        var existing = await _subscriptionRepository.GetActiveByCompanyAsync(companyId, cancellationToken);
        if (existing is not null)
        {
            return Result<SubscriptionSummary>.Fail("Company already has an active subscription.");
        }

        var now = DateTime.UtcNow;
        var subscription = Subscription.Create(companyId, plan.Id, now, SubscriptionStatus.PendingActivation);
        var cycle = CreateBillingCycle(subscription, plan.BillingCycle, now);
        subscription.AttachBillingCycle(cycle);

        if (trialDays.HasValue && trialDays.Value > 0)
        {
            var trial = Trial.Create(subscription, now, now.AddDays(trialDays.Value));
            subscription.AttachTrial(trial);
        }

        var resolvedSeatLimit = seatLimit ?? plan.SeatLimit ?? 0;
        if (resolvedSeatLimit > 0)
        {
            subscription.AttachSeatCount(SeatCount.Create(subscription, resolvedSeatLimit, 0));
        }

        await _subscriptionRepository.AddAsync(subscription, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (plan.PriceAmount <= 0)
        {
            subscription.Activate();
        }
        else
        {
            var chargeResult = await _webpayGateway.CreateChargeAsync(
                new WebpayChargeRequest(
                    companyId,
                    subscription.Id,
                    plan.PriceAmount,
                    plan.PriceCurrency,
                    $"Subscription onboarding for {plan.Name}"),
                cancellationToken);

            if (!chargeResult.Success)
            {
                return Result<SubscriptionSummary>.Fail(chargeResult.Error ?? "Webpay charge failed.");
            }

            subscription.Activate();
        }

        var featureResult = await ApplyPlanFeaturesAsync(companyId, plan.Id, requestedByUserId, cancellationToken);
        if (!featureResult.Success)
        {
            return Result<SubscriptionSummary>.Fail(featureResult.Error ?? "Failed to sync company features.");
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<SubscriptionSummary>.Ok(ToSummary(subscription));
    }

    public async Task<Result<SubscriptionSummary>> ChangePlanAsync(
        long companyId,
        long planId,
        long requestedByUserId,
        CancellationToken cancellationToken = default)
    {
        var plan = await _planRepository.GetByIdAsync(planId, cancellationToken);
        if (plan is null || !plan.IsActive)
        {
            return Result<SubscriptionSummary>.Fail("Plan not found or inactive.");
        }

        var subscription = await _subscriptionRepository.GetActiveByCompanyAsync(companyId, cancellationToken);
        if (subscription is null)
        {
            return Result<SubscriptionSummary>.Fail("Active subscription not found.");
        }

        subscription.UpdatePlan(plan.Id);
        subscription.AttachBillingCycle(CreateBillingCycle(subscription, plan.BillingCycle, DateTime.UtcNow));

        if (subscription.SeatCount is not null && plan.SeatLimit.HasValue)
        {
            subscription.SeatCount.UpdateLimit(plan.SeatLimit.Value);
        }

        if (plan.PriceAmount > 0)
        {
            var chargeResult = await _webpayGateway.CreateChargeAsync(
                new WebpayChargeRequest(
                    companyId,
                    subscription.Id,
                    plan.PriceAmount,
                    plan.PriceCurrency,
                    $"Subscription plan change to {plan.Name}"),
                cancellationToken);

            if (!chargeResult.Success)
            {
                return Result<SubscriptionSummary>.Fail(chargeResult.Error ?? "Webpay charge failed.");
            }
        }

        var featureResult = await ApplyPlanFeaturesAsync(companyId, plan.Id, requestedByUserId, cancellationToken);
        if (!featureResult.Success)
        {
            return Result<SubscriptionSummary>.Fail(featureResult.Error ?? "Failed to sync company features.");
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<SubscriptionSummary>.Ok(ToSummary(subscription));
    }

    public async Task<Result<SubscriptionSummary>> CancelAsync(
        long companyId,
        long requestedByUserId,
        string? reason,
        CancellationToken cancellationToken = default)
    {
        var subscription = await _subscriptionRepository.GetActiveByCompanyAsync(companyId, cancellationToken);
        if (subscription is null)
        {
            return Result<SubscriptionSummary>.Fail("Active subscription not found.");
        }

        subscription.Cancel(DateTime.UtcNow);

        var featureResult = await _companyFeatureManager.ReplaceSetAsync(
            companyId,
            Array.Empty<string>(),
            requestedByUserId,
            reason,
            "subscriptions:cancel",
            cancellationToken);

        if (!featureResult.Success)
        {
            return Result<SubscriptionSummary>.Fail(featureResult.Error ?? "Failed to disable company features.");
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<SubscriptionSummary>.Ok(ToSummary(subscription));
    }

    private BillingCycle CreateBillingCycle(Subscription subscription, BillingCycleType cycleType, DateTime startAt)
    {
        var endAt = cycleType switch
        {
            BillingCycleType.Monthly => startAt.AddMonths(1),
            BillingCycleType.Quarterly => startAt.AddMonths(3),
            BillingCycleType.Yearly => startAt.AddYears(1),
            _ => startAt.AddMonths(1)
        };

        return BillingCycle.Create(subscription, cycleType, startAt, endAt);
    }

    private async Task<Result<bool>> ApplyPlanFeaturesAsync(
        long companyId,
        long planId,
        long requestedByUserId,
        CancellationToken cancellationToken)
    {
        var features = await _planRepository.GetFeatureKeysAsync(planId, cancellationToken);
        return await _companyFeatureManager.ReplaceSetAsync(
            companyId,
            features,
            requestedByUserId,
            "Plan synchronization",
            "subscriptions:plan",
            cancellationToken);
    }

    private static SubscriptionSummary ToSummary(Subscription subscription)
        => new(
            subscription.Id,
            subscription.CompanyId,
            subscription.PlanId,
            subscription.Status.ToString(),
            subscription.StartedAt,
            subscription.CancelledAt);
}
