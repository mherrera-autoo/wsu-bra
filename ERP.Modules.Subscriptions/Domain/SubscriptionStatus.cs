namespace ERP.Modules.Subscriptions.Domain;

public enum SubscriptionStatus
{
    PendingActivation = 1,
    Active = 2,
    Cancelled = 3,
    PastDue = 4,
    Suspended = 5
}
