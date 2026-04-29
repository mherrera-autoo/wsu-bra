using ERP.Shared.Contracts;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ERP.Api.Swagger;

public sealed class GlobalErrorResponsesOperationFilter : IOperationFilter
{
    private const string JsonContentType = "application/json";

    private static readonly IReadOnlyDictionary<string, string> DefaultResponses =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["400"] = "Bad request.",
            ["401"] = "Unauthorized.",
            ["403"] = "Forbidden.",
            ["404"] = "Not found.",
            ["409"] = "Conflict.",
            ["422"] = "Unprocessable entity.",
            ["429"] = "Too many requests.",
            ["500"] = "Internal server error.",
            ["503"] = "Service unavailable."
        };

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation.Responses is null)
        {
            return;
        }

        var errorSchema = context.SchemaGenerator.GenerateSchema(typeof(ErrorResponse), context.SchemaRepository);

        foreach (var response in DefaultResponses)
        {
            if (operation.Responses.ContainsKey(response.Key))
            {
                continue;
            }

            operation.Responses.Add(response.Key, new OpenApiResponse
            {
                Description = response.Value,
                Content = new Dictionary<string, OpenApiMediaType>(StringComparer.Ordinal)
                {
                    [JsonContentType] = new OpenApiMediaType
                    {
                        Schema = errorSchema
                    }
                }
            });
        }
    }
}
