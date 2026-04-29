using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ERP.Notifications.Contracts.Notifications;

namespace ERP.Notifications.Infrastructure.Email;

public sealed class AzureAcsEmailSender : IEmailSender
{
    private readonly IOptionsMonitor<EmailOptions> _options;
    private readonly ILogger<AzureAcsEmailSender> _logger;

    public AzureAcsEmailSender(
        IOptionsMonitor<EmailOptions> options,
        ILogger<AzureAcsEmailSender> logger)
    {
        _options = options;
        _logger = logger;
    }

    public async Task SendAsync(EmailMessage message, CancellationToken ct = default)
    {
        _logger.LogWarning("AzureAcs provider not configured yet. Email not sent. To: {To}, Subject: {Subject}, CorrelationId: {CorrelationId}",
            message.To, message.Subject, message.CorrelationId);
        
        throw new NotImplementedException("AzureAcs provider not configured yet.");
    }
}