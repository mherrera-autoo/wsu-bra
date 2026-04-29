using ERP.EdgePublicApi.Configuration;
using ERP.EdgePublicApi.Middleware;
using ERP.EdgePublicApi.Services;
using ERP.EdgePublicApi.Swagger;
using ERP.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);
var isProduction = builder.Environment.IsProduction();

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Services
    .AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
            ErrorResponses.BadRequest(context.HttpContext, "invalid_payload", "Request payload is invalid.");
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.OperationFilter<EdgeApiHeaderOperationFilter>();
    options.CustomSchemaIds(type => type.FullName?.Replace('+', '.') ?? type.Name);
});

builder.Services.AddDbContext<ErpDbContext>(options =>
{
    if (builder.Environment.IsEnvironment("Testing"))
    {
        var testingConnection = builder.Configuration.GetConnectionString("EdgeTesting");
        if (string.IsNullOrWhiteSpace(testingConnection))
        {
            testingConnection = "Data Source=edge-public-api-tests.db";
        }

        options.UseSqlite(testingConnection);
        return;
    }

    var connectionString = builder.Configuration.GetConnectionString("Default");
    if (string.IsNullOrWhiteSpace(connectionString))
    {
        connectionString = builder.Configuration.GetConnectionString("ErpDatabase");
    }

    if (string.IsNullOrWhiteSpace(connectionString))
    {
        connectionString = Environment.GetEnvironmentVariable("ERP_DB");
    }

    if (string.IsNullOrWhiteSpace(connectionString))
    {
        if (isProduction)
        {
            throw new InvalidOperationException("Database connection string is not configured. Set ConnectionStrings:Default, ConnectionStrings:ErpDatabase, or ERP_DB.");
        }

        connectionString = "Host=localhost;Port=5432;Database=erp;Username=erp;Password=erp";
    }

    options.UseNpgsql(connectionString);
});

builder.Services.Configure<EdgePublicApiOptions>(builder.Configuration.GetSection(EdgePublicApiOptions.SectionName));
builder.Services.PostConfigure<EdgePublicApiOptions>(options =>
{
    if (string.IsNullOrWhiteSpace(options.ApiKey))
    {
        options.ApiKey = builder.Configuration["EDGE_PUBLIC_API_KEY"];
    }

    if (isProduction && string.IsNullOrWhiteSpace(options.ApiKey))
    {
        throw new InvalidOperationException("Edge public API key is not configured. Set EdgePublicApi:ApiKey or EDGE_PUBLIC_API_KEY.");
    }
});

builder.Services.AddScoped<EdgeEventIngestionService>();

var app = builder.Build();

app.UseRouting();

app.UseSwagger(options =>
{
    options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_1;
});
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "ERP Edge Public API v1");
    options.DisplayRequestDuration();
});

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<EdgeApiKeyMiddleware>();

app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

await app.RunAsync();

public partial class Program;
