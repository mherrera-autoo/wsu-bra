using Microsoft.Extensions.Logging;
using ERP.Notifications.Contracts.Notifications;

namespace ERP.Modules.Identity.Application.Services;

public class EmailSmokeTestService
{
    private readonly IEmailSender _emailSender;
    private readonly ILogger<EmailSmokeTestService> _logger;

    public EmailSmokeTestService(IEmailSender emailSender, ILogger<EmailSmokeTestService> logger)
    {
        _emailSender = emailSender;
        _logger = logger;
    }

    public async Task SendTestEmailAsync(string toEmail, CancellationToken ct = default)
    {
        var message = new EmailMessage(
            To: toEmail,
            Subject: "AutooERP - Email System Test",
            HtmlBody: $@"
                <h2>AutooERP Email System Test</h2>
                <p>This is a test email to verify that the email system is working correctly.</p>
                <p><strong>Sent at:</strong> {DateTime.UtcNow:O} UTC</p>
                <p><strong>Environment:</strong> Test</p>
                <hr>
                <p><em>This is an automated test message from AutooERP.</em></p>",
            TextBody: $@"
AutooERP Email System Test

This is a test email to verify that the email system is working correctly.

Sent at: {DateTime.UtcNow:O} UTC
Environment: Test

---
This is an automated test message from AutooERP.",
            CorrelationId: Guid.NewGuid().ToString()
        );

        _logger.LogInformation("Sending test email to {ToEmail} with CorrelationId: {CorrelationId}", toEmail, message.CorrelationId);

        try
        {
            await _emailSender.SendAsync(message, ct);
            _logger.LogInformation("Test email sent successfully to {ToEmail} with CorrelationId: {CorrelationId}", toEmail, message.CorrelationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send test email to {ToEmail} with CorrelationId: {CorrelationId}", toEmail, message.CorrelationId);
            throw;
        }
    }
}