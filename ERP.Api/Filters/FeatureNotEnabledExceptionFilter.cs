using ERP.Shared.Application;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ERP.Api.Filters;

public sealed class FeatureNotEnabledExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is not FeatureNotEnabledException)
        {
            return;
        }

        context.Result = new NotFoundResult();
        context.ExceptionHandled = true;
    }
}
