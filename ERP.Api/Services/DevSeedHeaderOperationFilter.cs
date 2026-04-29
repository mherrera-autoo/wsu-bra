using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ERP.Api.Services;

public sealed class DevSeedHeaderOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var relativePath = context.ApiDescription.RelativePath;
        var isDevSeed = string.Equals(relativePath, "api/dev/seed/base", StringComparison.OrdinalIgnoreCase)
            || string.Equals(relativePath, "api/dev/seed/accounting", StringComparison.OrdinalIgnoreCase)
            || string.Equals(relativePath, "api/dev/seed/prices", StringComparison.OrdinalIgnoreCase)
            || string.Equals(relativePath, "api/dev/seed/operation", StringComparison.OrdinalIgnoreCase);

        if (!isDevSeed)
        {
            return;
        }

        operation.Parameters ??= [];

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "X-DEV-SEED-KEY",
            In = ParameterLocation.Header,
            Required = true,
            Description = "Development seed key.",
            Schema = new OpenApiSchema
            {
                Type = JsonSchemaType.String
            }
        });
    }
}
