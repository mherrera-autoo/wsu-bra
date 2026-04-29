using ERP.Documents.Contracts.Storage;
using ERP.Documents.Infrastructure.BinaryStorage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ERP.Documents;

public static class DependencyInjection
{
    public static IServiceCollection AddDocuments(this IServiceCollection services, IConfiguration configuration)
    {
        var binaryStorageSection = configuration.GetSection("BinaryStorage");
        services.Configure<BinaryStorageOptions>(binaryStorageSection);

        services.AddTransient<S3BinaryStorage>();
        services.AddTransient<AzureBlobBinaryStorage>();
        services.AddTransient<MinioBinaryStorage>();
        services.AddTransient<LocalBinaryStorage>();

        services.AddSingleton<BinaryStorageRouter>();
        services.AddSingleton<IBinaryStorage>(provider => provider.GetRequiredService<BinaryStorageRouter>());

        return services;
    }
}
