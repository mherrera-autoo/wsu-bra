namespace ERP.Api.Contracts.Subscriptions;

public sealed record SubscriptionResponse(
    long Id,
    long CompanyId,
    long PlanId,
    string Status,
    DateTime StartedAt,
    DateTime? CancelledAt);
