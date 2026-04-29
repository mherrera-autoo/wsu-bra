namespace ERP.EdgePublicApi.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ArgumentException exception)
        {
            _logger.LogWarning(exception, "Invalid request argument.");
            await ErrorResponses.WriteAsync(context, StatusCodes.Status400BadRequest, "invalid_argument", exception.Message);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unhandled exception.");
            await ErrorResponses.WriteAsync(context, StatusCodes.Status500InternalServerError, "server_error", "An unexpected error occurred.");
        }
    }
}
