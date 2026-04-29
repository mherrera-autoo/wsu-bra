using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using ERP.Notifications.Contracts.Notifications;

namespace ERP.Notifications.Infrastructure.Email;

public sealed class EmailSenderRouter : IEmailSender
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IOptionsMonitor<EmailOptions> _options;
    private readonly ILogger<EmailSenderRouter> _logger;

    private IEmailSender _inner = null!;

    public EmailSenderRouter(IServiceProvider serviceProvider, IOptionsMonitor<EmailOptions> options, ILogger<EmailSenderRouter> logger)
    {
        _serviceProvider = serviceProvider;
        _options = options;
        _logger = logger;

        _inner = CreateSender(options.CurrentValue.Provider);
        
        _options.OnChange(newOptions =>
        {
            var oldProvider = Volatile.Read(ref _inner) switch
            {
                SmtpEmailSender => "Smtp",
                SendGridEmailSender => "SendGrid", 
                AzureAcsEmailSender => "AzureAcs",
                AwsSesEmailSender => "AwsSes",
                _ => "Unknown"
            };

            var newProvider = newOptions.Provider.ToString();
            var newSender = CreateSender(newOptions.Provider);
            
            Volatile.Write(ref _inner, newSender);
            
            _logger.LogInformation("Email provider changed from {OldProvider} to {NewProvider}", oldProvider, newProvider);
        });
    }

    private IEmailSender CreateSender(EmailProvider provider)
    {
        return provider switch
        {
            EmailProvider.Smtp => _serviceProvider.GetRequiredService<SmtpEmailSender>(),
            EmailProvider.SendGrid => _serviceProvider.GetRequiredService<SendGridEmailSender>(),
            EmailProvider.AzureAcs => _serviceProvider.GetRequiredService<AzureAcsEmailSender>(),
            EmailProvider.AwsSes => _serviceProvider.GetRequiredService<AwsSesEmailSender>(),
            _ => throw new NotSupportedException($"Email provider '{provider}' is not supported")
        };
    }

    public async Task SendAsync(EmailMessage message, CancellationToken ct = default)
    {
        var sender = Volatile.Read(ref _inner);
        await sender.SendAsync(message, ct);
    }
}