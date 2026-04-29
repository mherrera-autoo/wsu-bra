using Microsoft.AspNetCore.Mvc;

namespace ERP.EdgePublicApi.Middleware;

public static class ErrorResponses
{
    public static IActionResult Unauthorized(HttpContext context, string code, string message)
        => Result(StatusCodes.Status401Unauthorized, context, code, message);

    public static IActionResult BadRequest(HttpContext context, string code, string message)
        => Result(StatusCodes.Status400BadRequest, context, code, message);

    public static IActionResult TooManyRequests(HttpContext context, string code, string message)
        => Result(StatusCodes.Status429TooManyRequests, context, code, message);

    public static IActionResult InternalServerError(HttpContext context, string code, string message)
        => Result(StatusCodes.Status500InternalServerError, context, code, message);

    public static ObjectResult Result(int statusCode, HttpContext context, string code, string message)
    {
        return new ObjectResult(new EdgeApiErrorResponse(
            code,
            message,
            context.TraceIdentifier))
        {
            StatusCode = statusCode
        };
    }

    public static async Task WriteAsync(HttpContext context, int statusCode, string code, string message)
    {
        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new EdgeApiErrorResponse(code, message, context.TraceIdentifier));
    }
}

public sealed record EdgeApiErrorResponse(string Code, string Message, string TraceId);
