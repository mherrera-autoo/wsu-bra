namespace ERP.Modules.Accounting.Application.Services;

public interface IChartOfAccountsTemplateSource
{
    Task<IReadOnlyList<ChartOfAccountsTemplateEntry>> LoadAsync(string version, CancellationToken cancellationToken = default);
}

public sealed record ChartOfAccountsTemplateEntry
{
    public string Id { get; init; } = null!;
    public string TemplateId { get; init; } = null!;
    public string? ParentTemplateAccountId { get; init; }
    public string Code { get; init; } = null!;
    public string Name { get; init; } = null!;
    public string AccountType { get; init; } = null!;
    public string? SystemRole { get; init; }
    public int SortOrder { get; init; }
    public bool? IsSystemRequired { get; init; }
    public bool? IsActive { get; init; }
}
