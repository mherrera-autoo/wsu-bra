using ERP.Shared.Domain;

namespace ERP.Modules.Accounting.Domain;

public sealed class Account : CompanyEntity
{
    public long? TemplateId { get; private set; }
    public string Code { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public AccountType AccountType { get; private set; }
    public long? ParentId { get; private set; }
    public bool IsSystemRequired { get; private set; }
    public bool IsLocked { get; private set; }
    public int SortOrder { get; private set; }
    public bool IsPostable { get; private set; }
    public string? CurrencyCode { get; private set; }
    public bool IsActive { get; private set; }
    public string? SystemRole { get; private set; }
    public Guid? SourceTemplateAccountPublicId { get; private set; }

    private Account() { }

    public static Account Create(
        long companyId,
        string code,
        string name,
        AccountType type,
        long? parentId = null,
        bool isSystemRequired = false,
        bool isLocked = false,
        bool isActive = true,
        int sortOrder = 0,
        long? templateId = null,
        bool isPostable = true,
        string? currencyCode = null,
        string? systemRole = null,
        Guid? sourceTemplateAccountPublicId = null)
        => new()
        {
            CompanyId = companyId,
            TemplateId = templateId,
            Code = code.Trim(),
            Name = name.Trim(),
            AccountType = type,
            ParentId = parentId,
            IsSystemRequired = isSystemRequired,
            IsLocked = isLocked,
            SortOrder = sortOrder,
            IsPostable = isPostable,
            CurrencyCode = string.IsNullOrWhiteSpace(currencyCode) ? null : currencyCode.Trim(),
            IsActive = isActive,
            SystemRole = NormalizeSystemRole(systemRole),
            SourceTemplateAccountPublicId = sourceTemplateAccountPublicId
        };

    public void UpdateDetails(string code, string name, AccountType type, long? parentId, bool isActive, int sortOrder)
    {
        Code = code.Trim();
        Name = name.Trim();
        AccountType = type;
        ParentId = parentId;
        IsActive = isActive;
        SortOrder = sortOrder;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateNameAndStatus(string name, bool isActive)
    {
        Name = name.Trim();
        IsActive = isActive;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    private static string? NormalizeSystemRole(string? systemRole)
        => string.IsNullOrWhiteSpace(systemRole) ? null : systemRole.Trim();
}

public enum AccountType
{
    Asset,
    Liability,
    Equity,
    Revenue,
    Expense,
    Cost
}

public sealed class JournalEntry : CompanyEntity
{
    public long JournalId { get; private set; }
    public DateTime EntryDate { get; private set; }
    public DateTime PostingDate { get; private set; }
    public long PeriodId { get; private set; }
    public string? Description { get; private set; }
    public JournalEntryStatus Status { get; private set; }
    public string SourceModule { get; private set; } = null!;
    public string SourceDocumentId { get; private set; } = null!;
    public string SourceDocumentType { get; private set; } = null!;
    public long? ReversesEntryId { get; private set; }
    public long CreatedBy { get; private set; }
    public DateTime PostedAt { get; private set; }
    public long PostedBy { get; private set; }
    public List<JournalEntryLine> Lines { get; private set; } = new();

    private JournalEntry() { }

    public static JournalEntry CreatePosted(
        long companyId,
        long journalId,
        DateTime entryDate,
        DateTime postingDate,
        long periodId,
        string? description,
        string sourceModule,
        string sourceDocumentId,
        string sourceDocumentType,
        long createdBy,
        DateTime postedAt,
        long postedBy,
        long? reversesEntryId = null)
    {
        return new JournalEntry
        {
            CompanyId = companyId,
            JournalId = journalId,
            EntryDate = entryDate,
            PostingDate = postingDate,
            PeriodId = periodId,
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            Status = JournalEntryStatus.Posted,
            SourceModule = sourceModule.Trim(),
            SourceDocumentId = sourceDocumentId.Trim(),
            SourceDocumentType = sourceDocumentType.Trim(),
            CreatedBy = createdBy,
            PostedAt = postedAt,
            PostedBy = postedBy,
            ReversesEntryId = reversesEntryId
        };
    }

    public void AddLine(JournalEntryLine line)
    {
        Lines.Add(line ?? throw new ArgumentNullException(nameof(line)));
    }


    internal void AssignId(long id)
    {
        Id = id;
    }
}

public enum JournalEntryStatus
{
    Posted
}

public sealed class JournalEntryLine : CompanyEntity
{
    public long JournalEntryId { get; private set; }
    public long AccountId { get; private set; }
    public decimal Debit { get; private set; }
    public decimal Credit { get; private set; }
    public string CurrencyCode { get; private set; } = null!;
    public decimal FxRate { get; private set; }
    public decimal AmountInBaseCurrency { get; private set; }
    public long? CostCenterId { get; private set; }
    public long? ProjectId { get; private set; }

    private JournalEntryLine() { }

    public static JournalEntryLine Create(
        long companyId,
        long journalEntryId,
        long accountId,
        decimal debit,
        decimal credit,
        string currencyCode,
        decimal fxRate,
        decimal amountInBaseCurrency,
        long? costCenterId,
        long? projectId)
    {
        if (debit < 0 || credit < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(debit), "Debit/Credit must be >= 0.");
        }

        if (debit == 0 && credit == 0)
        {
            throw new InvalidOperationException("Either debit or credit must be > 0.");
        }

        if (string.IsNullOrWhiteSpace(currencyCode))
        {
            throw new ArgumentException("Currency code is required.", nameof(currencyCode));
        }

        return new JournalEntryLine
        {
            CompanyId = companyId,
            JournalEntryId = journalEntryId,
            AccountId = accountId,
            Debit = debit,
            Credit = credit,
            CurrencyCode = currencyCode.Trim(),
            FxRate = fxRate,
            AmountInBaseCurrency = amountInBaseCurrency,
            CostCenterId = costCenterId,
            ProjectId = projectId
        };
    }
}
