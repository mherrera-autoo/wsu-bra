using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ERP.Notifications.Contracts.Notifications;

namespace ERP.Notifications.Infrastructure.Email;

public sealed class SendGridEmailSender : IEmailSender
{
    private readonly IOptionsMonitor<EmailOptions> _options;
    private readonly ILogger<SendGridEmailSender> _logger;

    public SendGridEmailSender(
        IOptionsMonitor<EmailOptions> options,
        ILogger<SendGridEmailSender> logger)
    {
        _options = options;
        _logger = logger;
    }

    public async Task SendAsync(EmailMessage message, CancellationToken ct = default)
    {
        _logger.LogWarning("SendGrid provider not configured yet. Email not sent. To: {To}, Subject: {Subject}, CorrelationId: {CorrelationId}",
            message.To, message.Subject, message.CorrelationId);
        
        throw new NotImplementedException("SendGrid provider not configured yet.");
    }
}