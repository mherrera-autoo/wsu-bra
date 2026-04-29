using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ERP.Notifications.Contracts.Notifications;
using ERP.Notifications.Infrastructure.Email;

namespace ERP.Notifications;

public static class DependencyInjection
{
    public static IServiceCollection AddNotifications(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure options
        services.Configure<EmailOptions>(configuration.GetSection("Email"));

        // Add memory cache for OAuth tokens
        services.AddMemoryCache();

        // Configure HTTP client for Google OAuth
        services.AddHttpClient("GoogleOAuthToken", client =>
        {
            client.Timeout = TimeSpan.FromSeconds(15);
        });

        // Register services
        services.AddSingleton<IGoogleOAuthTokenService, GoogleOAuthTokenService>();
        
        // Register email senders as transient
        services.AddTransient<SmtpEmailSender>();
        services.AddTransient<SendGridEmailSender>();
        services.AddTransient<AzureAcsEmailSender>();
        services.AddTransient<AwsSesEmailSender>();
        
        // Register router as singleton
        services.AddSingleton<EmailSenderRouter>();
        services.AddSingleton<IEmailSender>(provider => provider.GetRequiredService<EmailSenderRouter>());

        // Soft validation warnings
        var emailOptions = configuration.GetSection("Email").Get<EmailOptions>();
        if (emailOptions?.Provider == EmailProvider.Smtp && string.IsNullOrEmpty(emailOptions.Smtp.Host))
        {
            // Note: This warning will be logged after the service provider is built
            // We can't log here without creating a service provider, which is anti-pattern
        }

        return services;
    }
}