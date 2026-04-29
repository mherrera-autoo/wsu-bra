using System.Globalization;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ERP.Api.Filters;

public sealed class CompanyIdInputFilter : IAsyncActionFilter
{
    private const string CompanyIdKey = "CompanyId";
    private const string ErrorMessage = "CompanyId must not be provided; it is resolved from JWT.";

    public Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (AllowsCompanyIdInput(context))
        {
            return next();
        }

        if (HasCompanyIdInput(context))
        {
            context.Result = new BadRequestObjectResult(new { error = ErrorMessage });
            return Task.CompletedTask;
        }

        return next();
    }

    private static bool AllowsCompanyIdInput(ActionExecutingContext context)
        => context.ActionDescriptor.EndpointMetadata.OfType<AllowCompanyIdInputAttribute>().Any();

    private static bool HasCompanyIdInput(ActionExecutingContext context)
        => HasCompanyIdInRoute(context)
           || HasCompanyIdInQuery(context)
           || HasCompanyIdInBody(context);

    private static bool HasCompanyIdInRoute(ActionExecutingContext context)
        => context.RouteData.Values.Keys.Any(key => string.Equals(key, CompanyIdKey, StringComparison.OrdinalIgnoreCase));

    private static bool HasCompanyIdInQuery(ActionExecutingContext context)
        => context.HttpContext.Request.Query.Keys.Any(key => string.Equals(key, CompanyIdKey, StringComparison.OrdinalIgnoreCase));

    private static bool HasCompanyIdInBody(ActionExecutingContext context)
    {
        foreach (var argument in context.ActionArguments)
        {
            if (string.Equals(argument.Key, CompanyIdKey, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (argument.Value is null)
            {
                continue;
            }

            if (HasCompanyIdProperty(argument.Value))
            {
                return true;
            }
        }

        foreach (var entry in context.ModelState)
        {
            if (entry.Key.EndsWith(CompanyIdKey, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static bool HasCompanyIdProperty(object value)
    {
        var type = value.GetType();
        if (type == typeof(string) || type.IsPrimitive)
        {
            return false;
        }

        var property = type.GetProperty(CompanyIdKey, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        if (property is null)
        {
            return false;
        }

        var propertyValue = property.GetValue(value);
        if (propertyValue is null)
        {
            return true;
        }

        if (propertyValue is string stringValue)
        {
            return !string.IsNullOrWhiteSpace(stringValue);
        }

        if (propertyValue is IFormattable formattable)
        {
            return !string.IsNullOrWhiteSpace(formattable.ToString(null, CultureInfo.InvariantCulture));
        }

        return true;
    }
}
