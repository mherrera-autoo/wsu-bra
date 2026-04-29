namespace ERP.Modules.Subscriptions.Application.Models;

public sealed record SubscriptionSummary(
    long Id,
    long CompanyId,
    long PlanId,
    string Status,
    DateTime StartedAt,
    DateTime? CancelledAt);
