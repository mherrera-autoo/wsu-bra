using ERP.Modules.Accounting.Domain;

namespace ERP.Api.Contracts.Accounting;

public sealed record CreateAccountRequest(
    string Code,
    string Name,
    AccountType AccountType,
    long? ParentId,
    bool IsActive = true,
    int SortOrder = 0);
