using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;
using ERP.Notifications.Contracts.Notifications;

namespace ERP.Notifications.Infrastructure.Email;

public sealed class SmtpEmailSender : IEmailSender
{
    private readonly IOptionsMonitor<EmailOptions> _options;
    private readonly IGoogleOAuthTokenService _googleTokenService;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(
        IOptionsMonitor<EmailOptions> options,
        IGoogleOAuthTokenService googleTokenService,
        ILogger<SmtpEmailSender> logger)
    {
        _options = options;
        _googleTokenService = googleTokenService;
        _logger = logger;
    }

    public async Task SendAsync(EmailMessage message, CancellationToken ct = default)
    {
        var options = _options.CurrentValue;
        var smtpOptions = options.Smtp;

        _logger.LogInformation(
            "Sending email via SMTP. To: {To}, Subject: {Subject}, AuthMode: {AuthMode}, Host: {Host}, Provider: Smtp, CorrelationId: {CorrelationId}",
            message.To, message.Subject, smtpOptions.AuthMode, smtpOptions.Host, message.CorrelationId);

        try
        {
            using var client = new SmtpClient();
            var secureSocketOptions = smtpOptions.UseTls ? SecureSocketOptions.StartTls : SecureSocketOptions.None;

            await client.ConnectAsync(smtpOptions.Host, smtpOptions.Port, secureSocketOptions, ct);

            // Authentication
            if (smtpOptions.AuthMode == SmtpAuthMode.Basic)
            {
                if (string.IsNullOrEmpty(smtpOptions.Username) || string.IsNullOrEmpty(smtpOptions.Password))
                {
                    throw new InvalidOperationException("SMTP Basic authentication requires Username and Password");
                }

                await client.AuthenticateAsync(smtpOptions.Username, smtpOptions.Password, ct);
                _logger.LogDebug("Authenticated via SMTP Basic");
            }
            else if (smtpOptions.AuthMode == SmtpAuthMode.OAuth2)
            {
                var oauth2Options = smtpOptions.OAuth2;
                if (string.IsNullOrEmpty(oauth2Options.ClientId) || 
                    string.IsNullOrEmpty(oauth2Options.ClientSecret) || 
                    string.IsNullOrEmpty(oauth2Options.RefreshToken) || 
                    string.IsNullOrEmpty(oauth2Options.UserEmail))
                {
                    throw new InvalidOperationException("SMTP OAuth2 authentication requires ClientId, ClientSecret, RefreshToken, and UserEmail");
                }

                var accessToken = await _googleTokenService.GetAccessTokenAsync(
                    oauth2Options.ClientId,
                    oauth2Options.ClientSecret,
                    oauth2Options.RefreshToken,
                    ct);

                var oauth2 = new SaslMechanismOAuth2(oauth2Options.UserEmail, accessToken);
                await client.AuthenticateAsync(oauth2, ct);
                _logger.LogDebug("Authenticated via SMTP OAuth2");
            }

            // Build email message
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(options.FromName, options.FromEmail));
            email.To.Add(MailboxAddress.Parse(message.To));

            if (!string.IsNullOrEmpty(message.ReplyTo))
            {
                email.ReplyTo.Add(MailboxAddress.Parse(message.ReplyTo));
            }

            email.Subject = message.Subject;

            // Add custom headers
            if (message.Headers != null)
            {
                foreach (var header in message.Headers)
                {
                    email.Headers.Add(header.Key, header.Value);
                }
            }

            // Build body
            var bodyBuilder = new BodyBuilder();
            
            if (!string.IsNullOrEmpty(message.TextBody))
            {
                bodyBuilder.TextBody = message.TextBody;
            }
            
            bodyBuilder.HtmlBody = message.HtmlBody;

            if (!string.IsNullOrEmpty(message.TextBody))
            {
                email.Body = new MultipartAlternative(
                    new TextPart(TextFormat.Plain) { Text = message.TextBody },
                    new TextPart(TextFormat.Html) { Text = message.HtmlBody });
            }
            else
            {
                email.Body = new TextPart(TextFormat.Html) { Text = message.HtmlBody };
            }

            await client.SendAsync(email, ct);

            _logger.LogInformation(
                "Email sent successfully. To: {To}, Subject: {Subject}, CorrelationId: {CorrelationId}",
                message.To, message.Subject, message.CorrelationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to send email via SMTP. To: {To}, Subject: {Subject}, CorrelationId: {CorrelationId}",
                message.To, message.Subject, message.CorrelationId);
            throw;
        }
    }
}