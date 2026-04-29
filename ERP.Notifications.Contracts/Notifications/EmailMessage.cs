namespace ERP.Notifications.Contracts.Notifications;

public sealed record EmailMessage(
    string To,
    string Subject,
    string HtmlBody,
    string? TextBody = null,
    string? ReplyTo = null,
    IReadOnlyDictionary<string, string>? Headers = null,
    string? CorrelationId = null
);