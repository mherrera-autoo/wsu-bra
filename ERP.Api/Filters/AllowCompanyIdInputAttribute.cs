using System;

namespace ERP.Api.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class AllowCompanyIdInputAttribute : Attribute
{
}
