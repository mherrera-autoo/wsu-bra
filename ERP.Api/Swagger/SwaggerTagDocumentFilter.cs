using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ERP.Api.Swagger;

public sealed class SwaggerTagDocumentFilter(Dictionary<string, SwaggerTagDefinition> tags) : IDocumentFilter
{
    private readonly Dictionary<string, SwaggerTagDefinition> _tags = tags;

    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        swaggerDoc.Tags ??= new HashSet<OpenApiTag>();

        foreach (var (tagKey, definition) in _tags)
        {
            var existing = swaggerDoc.Tags.FirstOrDefault(tag => tag.Name == tagKey);
            if (existing is null)
            {
                existing = new OpenApiTag { Name = tagKey };
                swaggerDoc.Tags.Add(existing);
            }

            existing.Description = definition.Description;
        }
    }
}
