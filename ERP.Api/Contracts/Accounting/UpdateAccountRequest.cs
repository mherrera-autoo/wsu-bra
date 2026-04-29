using ERP.Modules.Accounting.Domain;

namespace ERP.Api.Contracts.Accounting;

public sealed record UpdateAccountRequest(
    string Code,
    string Name,
    AccountType AccountType,
    long? ParentId,
    bool IsActive,
    int SortOrder);
