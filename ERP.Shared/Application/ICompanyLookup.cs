namespace ERP.Shared.Application;

public interface ICompanyLookup
{
    Task<bool> ExistsAsync(long companyId, CancellationToken cancellationToken = default);
}
