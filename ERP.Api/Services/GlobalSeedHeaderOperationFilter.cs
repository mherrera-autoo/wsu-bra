using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ERP.Api.Services;

public sealed class GlobalSeedHeaderOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var isGlobalSeed = string.Equals(context.ApiDescription.RelativePath, "api/seed/global", StringComparison.OrdinalIgnoreCase)
            || string.Equals(context.ApiDescription.RelativePath, "api/seed/features", StringComparison.OrdinalIgnoreCase)
            || string.Equals(context.ApiDescription.RelativePath, "api/seed/global/rbac", StringComparison.OrdinalIgnoreCase)
            || string.Equals(context.ApiDescription.RelativePath, "api/seed/global/rbac/otros", StringComparison.OrdinalIgnoreCase)
            || string.Equals(context.ApiDescription.RelativePath, "api/seed/global/accounting", StringComparison.OrdinalIgnoreCase)
            || string.Equals(context.ApiDescription.RelativePath, "api/seed/rfid", StringComparison.OrdinalIgnoreCase)
            || string.Equals(context.ApiDescription.RelativePath, "api/seed/rfid/uom", StringComparison.OrdinalIgnoreCase);

        if (!isGlobalSeed)
        {
            return;
        }

        operation.Parameters ??= [];

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "X-GLOBAL-SEED-KEY",
            In = ParameterLocation.Header,
            Required = true,
            Description = "Global seed key.",
            Schema = new OpenApiSchema
            {
                Type = JsonSchemaType.String
            }
        });
    }
}
