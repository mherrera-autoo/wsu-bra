namespace ERP.Notifications.Contracts.Notifications;

public interface IEmailSender
{
    Task SendAsync(EmailMessage message, CancellationToken ct = default);
}