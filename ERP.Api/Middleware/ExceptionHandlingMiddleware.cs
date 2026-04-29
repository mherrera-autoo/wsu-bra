using ERP.Shared.Contracts;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace ERP.Api.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private const string TransientFailureMessage = "An exception has been raised that is likely due to a transient failure";
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
        catch (Exception exception) when (IsTransientDatabaseException(exception))
        {
            if (context.Response.HasStarted)
            {
                throw;
            }

            _logger.LogError(exception, "Database unavailable.");

            context.Response.Clear();
            context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            context.Response.ContentType = "application/json";

            var errorResponse = CreateErrorResponse(
                context,
                code: "service_unavailable",
                title: "Database unavailable",
                detail: "The database is temporarily unavailable. Please try again later.");

            await context.Response.WriteAsJsonAsync(errorResponse);
        }
        catch (DbUpdateException exception)
        {
            if (context.Response.HasStarted)
            {
                throw;
            }

            _logger.LogError(exception, "Database update exception.");

            context.Response.Clear();
            context.Response.StatusCode = StatusCodes.Status409Conflict;
            context.Response.ContentType = "application/json";

            var detail = exception.InnerException?.Message ?? exception.Message;
            var errorResponse = CreateErrorResponse(
                context,
                code: "persistence_conflict",
                title: "Persistence conflict",
                detail);

            await context.Response.WriteAsJsonAsync(errorResponse);
        }
        catch (ArgumentException exception)
        {
            if (context.Response.HasStarted)
            {
                throw;
            }

            _logger.LogWarning(exception, "Invalid request argument.");

            context.Response.Clear();
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/json";

            var errorResponse = CreateErrorResponse(
                context,
                code: "invalid_argument",
                title: "Invalid argument",
                detail: exception.Message);

            await context.Response.WriteAsJsonAsync(errorResponse);
        }
        catch (InvalidOperationException exception)
        {
            if (context.Response.HasStarted)
            {
                throw;
            }

            _logger.LogWarning(exception, "Invalid operation.");

            context.Response.Clear();
            context.Response.StatusCode = StatusCodes.Status409Conflict;
            context.Response.ContentType = "application/json";

            var errorResponse = CreateErrorResponse(
                context,
                code: "invalid_operation",
                title: "Invalid operation",
                detail: exception.Message);

            await context.Response.WriteAsJsonAsync(errorResponse);
        }
        catch (Exception exception)
        {
            if (context.Response.HasStarted)
            {
                throw;
            }

            _logger.LogError(exception, "Unhandled exception.");

            context.Response.Clear();
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";

            var errorResponse = CreateErrorResponse(
                context,
                code: "server_error",
                title: "Internal server error",
                detail: "An unexpected error occurred.");

            await context.Response.WriteAsJsonAsync(errorResponse);
        }
    }

    private static bool IsTransientDatabaseException(Exception exception)
    {
        if (exception is NpgsqlException)
        {
            return true;
        }

        if (exception is InvalidOperationException invalidOperationException
            && invalidOperationException.Message.Contains(TransientFailureMessage, StringComparison.Ordinal))
        {
            return true;
        }

        return exception.InnerException is not null && IsTransientDatabaseException(exception.InnerException);
    }

    private static ErrorResponse CreateErrorResponse(
        HttpContext context,
        string code,
        string title,
        string? detail,
        List<ErrorDetail>? errors = null)
    {
        return new ErrorResponse
        {
            TraceId = context.TraceIdentifier,
            Code = code,
            Title = title,
            Detail = detail,
            Errors = errors,
            TimestampUtc = DateTime.UtcNow.ToString("O")
        };
    }
}
