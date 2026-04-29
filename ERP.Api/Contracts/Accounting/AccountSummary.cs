using ERP.Modules.Accounting.Domain;

namespace ERP.Api.Contracts.Accounting;

public sealed record AccountSummary(
    long Id,
    string Code,
    string Name,
    AccountType AccountType,
    long? ParentId,
    bool IsActive,
    bool IsSystemRequired,
    bool IsLocked,
    int SortOrder);
