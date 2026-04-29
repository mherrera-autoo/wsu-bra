using System.Text.Json;
using Microsoft.AspNetCore.Hosting;

namespace ERP.Api.Swagger;

public static class SwaggerTagConfigLoader
{
    public static Dictionary<string, SwaggerTagDefinition> Load(IWebHostEnvironment env)
    {
        var path = Path.Combine(env.ContentRootPath, "config", "swagger-tags.json");
        if (!File.Exists(path))
        {
            return new Dictionary<string, SwaggerTagDefinition>(StringComparer.Ordinal);
        }

        var json = File.ReadAllText(path);
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        var tags = JsonSerializer.Deserialize<Dictionary<string, SwaggerTagDefinition>>(json, options);

        return tags ?? new Dictionary<string, SwaggerTagDefinition>(StringComparer.Ordinal);
    }
}
