using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ERP.EdgePublicApi.Swagger;

public sealed class EdgeApiHeaderOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var relativePath = context.ApiDescription.RelativePath;
        if (string.IsNullOrWhiteSpace(relativePath)
            || !relativePath.StartsWith("api/v1/edge", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        operation.Parameters ??= [];

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "x-api-key",
            In = ParameterLocation.Header,
            Required = true,
            Description = "Static API key configured by environment.",
            Schema = new OpenApiSchema { Type = JsonSchemaType.String }
        });

        if (relativePath.StartsWith("api/v1/edge/events/batch", StringComparison.OrdinalIgnoreCase))
        {
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "Idempotency-Key",
                In = ParameterLocation.Header,
                Required = false,
                Description = "Recommended client request idempotency key (UUID).",
                Schema = new OpenApiSchema { Type = JsonSchemaType.String }
            });
        }

        if (relativePath.StartsWith("api/v1/edge/orders/active", StringComparison.OrdinalIgnoreCase)
            || relativePath.StartsWith("api/v1/edge/orders/ready", StringComparison.OrdinalIgnoreCase))
        {
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "If-None-Match",
                In = ParameterLocation.Header,
                Required = false,
                Description = "Optional ETag for cache validation.",
                Schema = new OpenApiSchema { Type = JsonSchemaType.String }
            });
        }
    }
}
