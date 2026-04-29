using ERP.Persistence;
using ERP.Modules.Identity.Application.Services;
using ERP.Api.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ERP.Shared.Application;

namespace ERP.Api.Integration.Tests;

public sealed class ApiWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName;
    private readonly IReadOnlyDictionary<string, string?>? _configurationOverrides;

    public ApiWebApplicationFactory(string databaseName, IReadOnlyDictionary<string, string?>? configurationOverrides = null)
    {
        _databaseName = databaseName;
        _configurationOverrides = configurationOverrides;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, config) =>
        {
            if (_configurationOverrides is not null)
            {
                config.AddInMemoryCollection(_configurationOverrides);
            }
        });
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<ErpDbContext>>();
            services.AddDbContext<ErpDbContext>(options =>
                options.UseInMemoryDatabase(_databaseName));

            services.RemoveAll<IRbacService>();
            services.AddSingleton<IRbacService, TestRbacService>();

            services.RemoveAll<IUnitOfWork>();
            services.AddScoped<IUnitOfWork, TestUnitOfWork>();

            // Configure JWT options with a longer key for tests
            services.Configure<ERP.Api.Authentication.JwtOptions>(options =>
            {
                options.SigningKey = "dev-secret-change-me-that-is-long-enough-for-hs256-algorithm";
            });
            services.Configure<ERP.Modules.Identity.Application.Services.JwtOptions>(options =>
            {
                options.SigningKey = "dev-secret-change-me-that-is-long-enough-for-hs256-algorithm";
            });

            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = TestAuthHandler.Scheme;
                    options.DefaultChallengeScheme = TestAuthHandler.Scheme;
                })
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.Scheme, _ => { });
        });
    }
}
