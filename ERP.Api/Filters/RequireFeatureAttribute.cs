using ERP.Modules.MasterData.Contracts;
using ERP.Shared.Application;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ERP.Api.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class RequireFeatureAttribute : Attribute, IAsyncActionFilter
{
    private readonly string _featureKey;

    public RequireFeatureAttribute(string featureKey)
    {
        _featureKey = featureKey;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var currentUserProvider = context.HttpContext.RequestServices.GetRequiredService<ICurrentUserProvider>();
        var featureService = context.HttpContext.RequestServices.GetRequiredService<IFeatureService>();

        var currentUser = currentUserProvider.GetCurrentUser();
        if (currentUser is null)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        if (!await featureService.IsEnabled(currentUser.CompanyId, _featureKey, context.HttpContext.RequestAborted))
        {
            context.Result = new NotFoundResult();
            return;
        }

        await next();
    }
}
