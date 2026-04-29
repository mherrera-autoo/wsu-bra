namespace ERP.Api.Contracts.Subscriptions;

public sealed record OnboardCompanySubscriptionRequest(
    long PlanId,
    int? SeatLimit,
    int? TrialDays);

public sealed record ChangeSubscriptionPlanRequest(long PlanId);

public sealed record CancelSubscriptionRequest(string? Reason);
