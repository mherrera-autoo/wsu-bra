namespace ERP.Modules.Identity.Contracts;

[Flags]
public enum CompanyAccessSource
{
    None = 0,
    DirectMembership = 1,
    HoldingWorkspace = 2,
    PortfolioLink = 4
}
