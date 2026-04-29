namespace ERP.Modules.Identity.Application.Services;

public class RbacException : Exception
{
    public string ErrorCode { get; }

    public RbacException(string errorCode, string message) : base(message)
    {
        ErrorCode = errorCode;
    }
}

public class CompanyContextRequiredException : RbacException
{
    public CompanyContextRequiredException() : base("company_context_required", "Company context is required for this permission")
    {
    }
}

public class OrganizationContextRequiredException : RbacException
{
    public OrganizationContextRequiredException() : base("organization_context_required", "Organization context is required for this permission")
    {
    }
}

public class CompanyMembershipRequiredException : RbacException
{
    public CompanyMembershipRequiredException() : base("company_membership_required", "Active company membership is required for this operation")
    {
    }
}