using Amazon;
using Amazon.Runtime;
using Amazon.SimpleEmailV2;
using Amazon.SimpleEmailV2.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ERP.Notifications.Contracts.Notifications;

namespace ERP.Notifications.Infrastructure.Email;

public sealed class AwsSesEmailSender : IEmailSender
{
    private readonly IOptionsMonitor<EmailOptions> _options;
    private readonly ILogger<AwsSesEmailSender> _logger;

    public AwsSesEmailSender(
        IOptionsMonitor<EmailOptions> options,
        ILogger<AwsSesEmailSender> logger)
    {
        _options = options;
        _logger = logger;
    }

    public async Task SendAsync(EmailMessage message, CancellationToken ct = default)
    {
        var options = _options.CurrentValue;
        var sesOptions = options.AwsSes;

        if (string.IsNullOrWhiteSpace(sesOptions.AccessKey) ||
            string.IsNullOrWhiteSpace(sesOptions.SecretKey) ||
            string.IsNullOrWhiteSpace(sesOptions.Region))
        {
            throw new InvalidOperationException("AWS SES requires AccessKey, SecretKey, and Region.");
        }

        if (string.IsNullOrWhiteSpace(options.FromEmail))
        {
            throw new InvalidOperationException("AWS SES requires Email:FromEmail to be configured.");
        }

        var fromAddress = string.IsNullOrWhiteSpace(options.FromName)
            ? options.FromEmail
            : $"{options.FromName} <{options.FromEmail}>";

        _logger.LogInformation(
            "Sending email via AWS SES. To: {To}, Subject: {Subject}, Region: {Region}, Provider: AwsSes, CorrelationId: {CorrelationId}",
            message.To, message.Subject, sesOptions.Region, message.CorrelationId);

        try
        {
            var credentials = new BasicAWSCredentials(sesOptions.AccessKey, sesOptions.SecretKey);
            var region = RegionEndpoint.GetBySystemName(sesOptions.Region);

            using var client = new AmazonSimpleEmailServiceV2Client(credentials, region);

            var request = new SendEmailRequest
            {
                FromEmailAddress = fromAddress,
                Destination = new Destination
                {
                    ToAddresses = new List<string> { message.To }
                },
                Content = new EmailContent
                {
                    Simple = new Message
                    {
                        Subject = new Content { Data = message.Subject },
                        Body = new Body
                        {
                            Html = new Content { Data = message.HtmlBody },
                            Text = string.IsNullOrWhiteSpace(message.TextBody) ? null : new Content { Data = message.TextBody }
                        }
                    }
                }
            };

            if (!string.IsNullOrWhiteSpace(message.ReplyTo))
            {
                request.ReplyToAddresses = new List<string> { message.ReplyTo };
            }

            await client.SendEmailAsync(request, ct);

            _logger.LogInformation(
                "Email sent successfully via AWS SES. To: {To}, Subject: {Subject}, CorrelationId: {CorrelationId}",
                message.To, message.Subject, message.CorrelationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to send email via AWS SES. To: {To}, Subject: {Subject}, CorrelationId: {CorrelationId}",
                message.To, message.Subject, message.CorrelationId);
            throw;
        }
    }
}
