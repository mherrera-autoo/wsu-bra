namespace ERP.Modules.Wsu.Application.Repositories;

public interface IOperatorEnrollmentValidator
{
    Task<bool> IsEnrolledAsync(Guid companyPublicId, string operatorCode, CancellationToken cancellationToken = default);
}
