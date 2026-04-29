using System;
using System.Text.Json;
using ERP.Modules.Billing.Domain;
using ERP.Modules.Cash.Domain;
using ERP.Documents.Domain;
using ERP.Modules.Identity.Domain;
using ERP.Modules.Integrations.Contracts;
using ERP.Modules.Inventory.Domain;
using ERP.Modules.MasterData.Domain;
using ERP.Modules.Pricing.Domain;
using ERP.Modules.Purchasing.Domain;
using ERP.Modules.Sales.Domain;
using ERP.Modules.Subscriptions.Domain;
using ERP.Modules.Tax.Domain;
using ERP.Modules.Users.Domain;
using ERP.Workflows.Domain;
using ERP.Modules.Wms.Domain;
using ERP.Modules.Rfid.Domain;
using ERP.Modules.Wsu.Domain;
using ERP.Shared.Domain;
using ERP.Shared.Domain.ValueObjects;
using ERP.Persistence.Configurations.Identity;
using ERP.Persistence.Configurations.MasterData;
using ERP.Persistence.Configurations.Wsu;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NpgsqlTypes;

namespace ERP.Persistence;

public class ErpDbContext : DbContext
{
    private const string PharmaSchema = "pharma";
    private const string WmsSchema = "wms";
    private const string DocumentSchema = "documents";
    private const string SalesSchema = "sales";
    private const string RfidSchema = "rfid";
    private const string WsuSchema = "wsu";
    private static readonly JsonSerializerOptions DocumentSearchJsonOptions = new(JsonSerializerDefaults.Web);

    public ErpDbContext(DbContextOptions<ErpDbContext> options) : base(options) { }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductSupplier> ProductSuppliers => Set<ProductSupplier>();
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<TaxEntity> TaxEntities => Set<TaxEntity>();
    public DbSet<UnitOfMeasure> UnitOfMeasures => Set<UnitOfMeasure>();
    public DbSet<UnitOfMeasureTranslation> UnitOfMeasureTranslations => Set<UnitOfMeasureTranslation>();
    public DbSet<UnitOfMeasureExternalMapping> UnitOfMeasureExternalMappings => Set<UnitOfMeasureExternalMapping>();
    public DbSet<CompanyUnitOfMeasure> CompanyUnitsOfMeasure => Set<CompanyUnitOfMeasure>();
    public DbSet<ProductUnitConversion> ProductUnitConversions => Set<ProductUnitConversion>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<Country> Countries => Set<Country>();
    public DbSet<Currency> Currencies => Set<Currency>();
    public DbSet<CompanyCurrency> CompanyCurrencies => Set<CompanyCurrency>();
    public DbSet<Subdivision> Subdivisions => Set<Subdivision>();
    public DbSet<City> Cities => Set<City>();
    public DbSet<Locality> Localities => Set<Locality>();
    public DbSet<Address> Addresses => Set<Address>();
    public DbSet<PriceList> PriceLists => Set<PriceList>();
    public DbSet<PriceListItem> PriceListItems => Set<PriceListItem>();
    public DbSet<PricingPolicy> PricingPolicies => Set<PricingPolicy>();
    public DbSet<Stock> Stocks => Set<Stock>();
    public DbSet<InventoryMovement> InventoryMovements => Set<InventoryMovement>();
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<PurchaseOrderLine> PurchaseOrderLines => Set<PurchaseOrderLine>();
    public DbSet<PurchaseSuggestion> PurchaseSuggestions => Set<PurchaseSuggestion>();
    public DbSet<PurchaseSuggestionLine> PurchaseSuggestionLines => Set<PurchaseSuggestionLine>();
    public DbSet<GoodsReceipt> GoodsReceipts => Set<GoodsReceipt>();
    public DbSet<GoodsReceiptLine> GoodsReceiptLines => Set<GoodsReceiptLine>();
    public DbSet<SalesDocument> SalesDocuments => Set<SalesDocument>();
    public DbSet<SalesDocumentLine> SalesDocumentLines => Set<SalesDocumentLine>();
    public DbSet<SalesQuote> SalesQuotes => Set<SalesQuote>();
    public DbSet<SalesQuoteLine> SalesQuoteLines => Set<SalesQuoteLine>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<DocumentVersion> DocumentVersions => Set<DocumentVersion>();
    public DbSet<DocumentLink> DocumentLinks => Set<DocumentLink>();
    public DbSet<BankAccount> BankAccounts => Set<BankAccount>();
    public DbSet<BankStatement> BankStatements => Set<BankStatement>();
    public DbSet<BankTransaction> BankTransactions => Set<BankTransaction>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Receipt> Receipts => Set<Receipt>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceLine> InvoiceLines => Set<InvoiceLine>();
    public DbSet<CreditNote> CreditNotes => Set<CreditNote>();
    public DbSet<DebitNote> DebitNotes => Set<DebitNote>();
    public DbSet<Tax> Taxes => Set<Tax>();
    public DbSet<TaxGroup> TaxGroups => Set<TaxGroup>();
    public DbSet<TaxRule> TaxRules => Set<TaxRule>();
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<User> Users => Set<User>();
    public DbSet<UserSession> UserSessions => Set<UserSession>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<CompanyUser> CompanyUsers => Set<CompanyUser>();
    public DbSet<OrganizationMember> OrganizationMembers => Set<OrganizationMember>();
    public DbSet<CompanyLink> CompanyLinks => Set<CompanyLink>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<RoleAssignment> RoleAssignments => Set<RoleAssignment>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<UserAuditLog> UserAuditLogs => Set<UserAuditLog>();
    public DbSet<DocumentSearchEntry> DocumentSearchEntries => Set<DocumentSearchEntry>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
    public DbSet<UserPasswordHistory> UserPasswordHistory => Set<UserPasswordHistory>();
    public DbSet<CompanyFeature> CompanyFeatures => Set<CompanyFeature>();
    public DbSet<CompanyFeatureAudit> CompanyFeatureAudits => Set<CompanyFeatureAudit>();
    public DbSet<FeatureCatalog> FeatureCatalog => Set<FeatureCatalog>();
    public DbSet<WarehouseLocation> WarehouseLocations => Set<WarehouseLocation>();
    public DbSet<WarehouseOperation> WarehouseOperations => Set<WarehouseOperation>();
    public DbSet<DispatchConfirmation> DispatchConfirmations => Set<DispatchConfirmation>();
    public DbSet<Plan> Plans => Set<Plan>();
    public DbSet<PlanFeature> PlanFeatures => Set<PlanFeature>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<BillingCycle> BillingCycles => Set<BillingCycle>();
    public DbSet<Trial> Trials => Set<Trial>();
    public DbSet<SeatCount> SeatCounts => Set<SeatCount>();
    public DbSet<SubscriptionUsage> SubscriptionUsages => Set<SubscriptionUsage>();
    public DbSet<WorkflowDefinition> WorkflowDefinitions => Set<WorkflowDefinition>();
    public DbSet<WorkflowState> WorkflowStates => Set<WorkflowState>();
    public DbSet<WorkflowTransition> WorkflowTransitions => Set<WorkflowTransition>();
    public DbSet<WorkflowInstance> WorkflowInstances => Set<WorkflowInstance>();
    public DbSet<WorkflowTask> WorkflowTasks => Set<WorkflowTask>();
    public DbSet<WorkflowHistory> WorkflowHistories => Set<WorkflowHistory>();
    public DbSet<RfidTag> RfidTags => Set<RfidTag>();
    public DbSet<RfidEventInbox> RfidEventInboxes => Set<RfidEventInbox>();
    public DbSet<EdgeBusinessEvent> EdgeBusinessEvents => Set<EdgeBusinessEvent>();
    public DbSet<RfidAccessSession> RfidAccessSessions => Set<RfidAccessSession>();
    public DbSet<RfidMovementLink> RfidMovementLinks => Set<RfidMovementLink>();
    public DbSet<Operator> Operators => Set<Operator>();
    public DbSet<OperatorCredential> OperatorCredentials => Set<OperatorCredential>();
    public DbSet<Order> WsuOrders => Set<Order>();
    public DbSet<OrderItem> WsuOrderItems => Set<OrderItem>();
    public DbSet<OrderItemConsumption> WsuOrderItemConsumptions => Set<OrderItemConsumption>();
    public DbSet<OrderItemReconciliation> WsuOrderItemReconciliations => Set<OrderItemReconciliation>();
    public DbSet<OrderItemEpcAssignment> WsuOrderItemEpcAssignments => Set<OrderItemEpcAssignment>();
    public DbSet<WsuOrderMovementOperator> WsuOrderMovementOperators => Set<WsuOrderMovementOperator>();
    public DbSet<WsuInventoryMovement> WsuInventoryMovements => Set<WsuInventoryMovement>();
    public DbSet<WsuInventoryMovementOperator> WsuInventoryMovementOperators => Set<WsuInventoryMovementOperator>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var isNpgsql = string.Equals(Database.ProviderName, "Npgsql.EntityFrameworkCore.PostgreSQL", StringComparison.Ordinal);

        ConfigureCompanies(modelBuilder);
        ConfigureMasterData(modelBuilder);
        ConfigurePricing(modelBuilder);
        ConfigureInventory(modelBuilder);
        ConfigurePurchasing(modelBuilder);
        ConfigureSales(modelBuilder);
        ConfigureBilling(modelBuilder);
        ConfigureTax(modelBuilder);
        ConfigureCash(modelBuilder);
        ConfigureDocuments(modelBuilder);
        ConfigureUsers(modelBuilder);
        ConfigureIntegrations(modelBuilder);
        ConfigureIdentity(modelBuilder);
        ConfigureDocumentSearch(modelBuilder, isNpgsql);
        ConfigureAudit(modelBuilder);
        ConfigureCompanyFeatures(modelBuilder);
        ConfigureSubscriptions(modelBuilder);
        ConfigureWorkflows(modelBuilder);
        ConfigureRfid(modelBuilder);
        ConfigureWsu(modelBuilder);
        EnsureWmsSchemaMappings(modelBuilder);
        EnsureRfidSchemaMappings(modelBuilder);
        EnsureWsuSchemaMappings(modelBuilder);
    }

    private static void ConfigureWsu(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new OrderConfiguration());
        modelBuilder.ApplyConfiguration(new OrderItemConfiguration());
        modelBuilder.ApplyConfiguration(new OrderItemConsumptionConfiguration());
        modelBuilder.ApplyConfiguration(new OrderItemReconciliationConfiguration());
        modelBuilder.ApplyConfiguration(new OrderItemEpcAssignmentConfiguration());
        modelBuilder.ApplyConfiguration(new WsuOrderMovementOperatorConfiguration());
        modelBuilder.ApplyConfiguration(new WsuInventoryMovementConfiguration());
        modelBuilder.ApplyConfiguration(new WsuInventoryMovementOperatorConfiguration());
    }

    private static void EnsureWsuSchemaMappings(ModelBuilder modelBuilder)
    {
        var invalidEntities = modelBuilder.Model.GetEntityTypes()
            .Where(entityType => entityType.ClrType.Namespace == "ERP.Modules.Wsu.Domain"
                && !entityType.IsOwned())
            .Where(entityType => !string.Equals(entityType.GetSchema(), WsuSchema, StringComparison.Ordinal))
            .Select(entityType => $"{entityType.ClrType.Name} -> {entityType.GetSchema() ?? "<default>"}")
            .ToList();

        if (invalidEntities.Count == 0)
        {
            return;
        }

        throw new InvalidOperationException(
            $"WSU entities must map to schema '{WsuSchema}'. Invalid mappings: {string.Join(", ", invalidEntities)}");
    }

    private static void ConfigureRfid(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RfidTag>(entity =>
        {
            entity.ToTable("Tags", RfidSchema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CompanyPublicId).IsRequired();
            entity.Property(e => e.WarehousePublicId);
            entity.Property(e => e.Epc).IsRequired().HasMaxLength(128);
            entity.Property(e => e.Status).HasConversion<int>().IsRequired();
            entity.Property(e => e.Notes);
            entity.HasIndex(e => new { e.CompanyPublicId, e.Epc })
                .IsUnique()
                .HasFilter("\"WarehousePublicId\" IS NULL");
            entity.HasIndex(e => new { e.CompanyPublicId, e.WarehousePublicId, e.Epc })
                .IsUnique()
                .HasFilter("\"WarehousePublicId\" IS NOT NULL");
        });

        modelBuilder.Entity<RfidEventInbox>(entity =>
        {
            entity.ToTable("EventInbox", RfidSchema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CompanyPublicId).IsRequired();
            entity.Property(e => e.EventId).IsRequired().HasMaxLength(128);
            entity.Property(e => e.EventType).HasConversion<int>().IsRequired();
            entity.Property(e => e.DeviceCode).IsRequired().HasMaxLength(128);
            entity.Property(e => e.Confidence).HasPrecision(5, 4);
            entity.Property(e => e.PayloadJson).IsRequired();
            entity.Property(e => e.ProcessingStatus).HasConversion<int>().IsRequired();
            entity.Property(e => e.Error);
            entity.HasIndex(e => new { e.CompanyPublicId, e.EventId }).IsUnique();
            entity.HasIndex(e => new { e.CompanyPublicId, e.OccurredAt });
        });

        modelBuilder.Entity<EdgeBusinessEvent>(entity =>
        {
            entity.ToTable("EdgeBusinessEvents", RfidSchema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CompanyPublicId).IsRequired();
            entity.Property(e => e.EdgeNodeId).IsRequired().HasMaxLength(128);
            entity.Property(e => e.EventId).IsRequired().HasMaxLength(64);
            entity.Property(e => e.EventType).IsRequired().HasMaxLength(128);
            entity.Property(e => e.SchemaVersion).IsRequired();
            entity.Property(e => e.OccurredAtUtc).HasColumnType("timestamp with time zone").IsRequired();
            entity.Property(e => e.OrderId).HasColumnType("uuid");
            entity.Property(e => e.EpcHex).IsRequired().HasMaxLength(128);
            entity.Property(e => e.MovementType).HasMaxLength(32);
            entity.Property(e => e.FromZoneId).HasMaxLength(128);
            entity.Property(e => e.ToZoneId).HasMaxLength(128);
            entity.Property(e => e.ZoneId).HasMaxLength(128);
            entity.Property(e => e.ReaderId).IsRequired().HasMaxLength(128);
            entity.Property(e => e.AntennaId);
            entity.Property(e => e.Rssi).HasPrecision(8, 3);
            entity.Property(e => e.Reason);
            entity.Property(e => e.MetadataJson).IsRequired();
            entity.Property(e => e.ReceivedAtUtc).HasColumnType("timestamp with time zone").IsRequired();
            entity.HasIndex(e => e.EventId).IsUnique();
            entity.HasIndex(e => new { e.EdgeNodeId, e.OccurredAtUtc });
            entity.HasIndex(e => new { e.CompanyPublicId, e.OccurredAtUtc });
        });

        modelBuilder.Entity<RfidAccessSession>(entity =>
        {
            entity.ToTable("AccessSessions", RfidSchema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PublicId).IsRequired();
            entity.Property(e => e.CompanyPublicId).IsRequired();
            entity.Property(e => e.SessionId).HasMaxLength(128);
            entity.Property(e => e.DeviceCode).IsRequired().HasMaxLength(128);
            entity.Property(e => e.Status).HasConversion<int>().IsRequired();
            entity.Property(e => e.StartedAt).HasColumnType("timestamp with time zone").IsRequired();
            entity.Property(e => e.EndedAt).HasColumnType("timestamp with time zone");
            entity.Property(e => e.CreatedAt).HasColumnType("timestamp with time zone").IsRequired();
            entity.Property(e => e.UpdatedAt).HasColumnType("timestamp with time zone").IsRequired();
            entity.HasOne(e => e.Operator)
                .WithMany()
                .HasForeignKey(e => e.OperatorId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(e => e.PublicId).IsUnique();
            entity.HasIndex(e => new { e.CompanyPublicId, e.SessionId }).IsUnique();
            entity.HasIndex(e => new { e.CompanyPublicId, e.Status });
            entity.HasIndex(e => new { e.CompanyPublicId, e.StartedAt }).IsDescending(false, true);
            entity.HasIndex(e => new { e.CompanyPublicId, e.WarehousePublicId, e.StartedAt }).IsDescending(false, false, true);
            entity.HasIndex(e => new { e.CompanyPublicId, e.OperatorId, e.StartedAt }).IsDescending(false, false, true);
            entity.HasIndex(e => new { e.CompanyPublicId, e.DeviceCode, e.StartedAt }).IsDescending(false, false, true);
        });

        modelBuilder.Entity<RfidMovementLink>(entity =>
        {
            entity.ToTable("MovementLinks", RfidSchema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CompanyPublicId).IsRequired();
            entity.Property(e => e.EventId).IsRequired().HasMaxLength(128);
            entity.Property(e => e.SessionId).HasMaxLength(128);
            entity.Property(e => e.Epc).IsRequired().HasMaxLength(128);
        });

        modelBuilder.Entity<Operator>(entity =>
        {
            entity.ToTable("Operators", RfidSchema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PublicId).IsRequired();
            entity.Property(e => e.CompanyPublicId).IsRequired();
            entity.Property(e => e.Code).IsRequired().HasMaxLength(64);
            entity.Property(e => e.FullName).IsRequired().HasMaxLength(160);
            entity.Property(e => e.DocumentId).HasMaxLength(32);
            entity.Property(e => e.IsActive).HasDefaultValue(true).IsRequired();
            entity.Property(e => e.CreatedAt).HasColumnType("timestamp with time zone").IsRequired();
            entity.Property(e => e.UpdatedAt).HasColumnType("timestamp with time zone").IsRequired();
            entity.HasIndex(e => e.PublicId).IsUnique();
            entity.HasIndex(e => new { e.CompanyPublicId, e.Code }).IsUnique();
            entity.HasIndex(e => new { e.CompanyPublicId, e.FullName });
            entity.HasIndex(e => new { e.CompanyPublicId, e.DocumentId })
                .IsUnique()
                .HasFilter("\"DocumentId\" IS NOT NULL");
        });

        modelBuilder.Entity<OperatorCredential>(entity =>
        {
            entity.ToTable("OperatorCredentials", RfidSchema, tableBuilder =>
            {
                tableBuilder.HasCheckConstraint("CK_OperatorCredentials_FaceOrNfc", "\"FaceTemplateId\" IS NOT NULL OR \"NfcCardUid\" IS NOT NULL");
            });
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PublicId).IsRequired();
            entity.Property(e => e.CompanyPublicId).IsRequired();
            entity.Property(e => e.IsActive).HasDefaultValue(true).IsRequired();
            entity.Property(e => e.CreatedAt).HasColumnType("timestamp with time zone").IsRequired();
            entity.Property(e => e.UpdatedAt).HasColumnType("timestamp with time zone").IsRequired();
            entity.HasOne(e => e.Operator)
                .WithMany()
                .HasForeignKey(e => e.OperatorId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(e => e.PublicId).IsUnique();
            entity.HasIndex(e => e.OperatorId);
            entity.HasIndex(e => new { e.CompanyPublicId, e.FaceTemplateId })
                .IsUnique()
                .HasFilter("\"FaceTemplateId\" IS NOT NULL");
            entity.HasIndex(e => new { e.CompanyPublicId, e.NfcCardUid })
                .IsUnique()
                .HasFilter("\"NfcCardUid\" IS NOT NULL");
        });
    }

    private static void EnsureRfidSchemaMappings(ModelBuilder modelBuilder)
    {
        var rfidEntities = modelBuilder.Model.GetEntityTypes()
            .Where(entityType => entityType.ClrType.Namespace == "ERP.Modules.Rfid.Domain")
            .Where(entityType => entityType.GetTableName() is not null);

        foreach (var entityType in rfidEntities)
        {
            entityType.SetSchema(RfidSchema);
        }
    }

    private static void ConfigureWorkflows(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WorkflowDefinition>(entity =>
        {
            entity.ToTable("WorkflowDefinitions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.DocumentType).IsRequired().HasMaxLength(200);
            entity.Property(e => e.IsActive).IsRequired();
            entity.HasIndex(e => new { e.DocumentType, e.IsActive });
            entity.HasMany(e => e.States)
                .WithOne()
                .HasForeignKey(state => state.WorkflowDefinitionId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(e => e.Transitions)
                .WithOne()
                .HasForeignKey(transition => transition.WorkflowDefinitionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<WorkflowState>(entity =>
        {
            entity.ToTable("WorkflowStates");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.AssignedRole).HasMaxLength(200);
            entity.HasIndex(e => new { e.WorkflowDefinitionId, e.Name }).IsUnique();
        });

        modelBuilder.Entity<WorkflowTransition>(entity =>
        {
            entity.ToTable("WorkflowTransitions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ActionType).HasConversion<int>().IsRequired();
            entity.Property(e => e.RuleJson);
            entity.HasOne(e => e.FromState)
                .WithMany()
                .HasForeignKey(e => e.FromStateId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.ToState)
                .WithMany()
                .HasForeignKey(e => e.ToStateId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(e => new { e.WorkflowDefinitionId, e.FromStateId });
        });

        modelBuilder.Entity<WorkflowInstance>(entity =>
        {
            entity.ToTable("WorkflowInstances");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.DocumentType).IsRequired().HasMaxLength(200);
            entity.Property(e => e.DocumentId).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Status).HasConversion<int>().IsRequired();
            entity.Property(e => e.ContextDataJson);
            entity.HasIndex(e => new { e.DocumentType, e.DocumentId }).IsUnique();
        });

        modelBuilder.Entity<WorkflowTask>(entity =>
        {
            entity.ToTable("WorkflowTasks");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Status).HasConversion<int>().IsRequired();
            entity.Property(e => e.CompletedAction).HasConversion<int?>();
            entity.Property(e => e.AssignedRole).HasMaxLength(200);
            entity.HasIndex(e => new { e.WorkflowInstanceId, e.Status });
        });

        modelBuilder.Entity<WorkflowHistory>(entity =>
        {
            entity.ToTable("WorkflowHistories");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ActionType).HasConversion<int>().IsRequired();
            entity.Property(e => e.PerformedBy).HasMaxLength(256);
            entity.HasIndex(e => e.WorkflowInstanceId);
        });
    }

    private static void ConfigureCompanies(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Company>(entity =>
        {
            entity.ToTable("Companies");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.OrganizationId).IsRequired();
            entity.Property(e => e.TaxEntityId).IsRequired();
            entity.Property(e => e.Name).IsRequired();
            entity.HasIndex(e => new { e.OrganizationId, e.Name }).IsUnique();
            entity.HasIndex(e => e.TaxEntityId);
            entity.HasOne<Organization>()
                .WithMany()
                .HasForeignKey(e => e.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Organization>(entity =>
        {
            entity.ToTable("Organizations", "iam");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AccountType).HasConversion<string>().IsRequired();
            entity.Property(e => e.DisplayName).HasMaxLength(256);
            entity.HasIndex(e => e.AccountType);
        });

        modelBuilder.Entity<TaxEntity>(entity =>
        {
            entity.ToTable("TaxEntities", "iam");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CompanyPublicId).IsRequired();
            entity.Property(e => e.TaxId).IsRequired().HasMaxLength(32);
            entity.Property(e => e.DisplayName).HasMaxLength(256);
            entity.HasIndex(e => e.TaxId).IsUnique();
            entity.HasIndex(e => e.CompanyPublicId);
            entity.HasAlternateKey(e => new { e.CompanyPublicId, e.Id });
        });
    }

    private static void ConfigureCompanyReference<TEntity>(EntityTypeBuilder<TEntity> entity)
        where TEntity : class
    {
        entity.HasOne<Company>()
            .WithMany()
            .HasForeignKey("CompanyId")
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureMasterData(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new CountryConfiguration());
        modelBuilder.ApplyConfiguration(new CurrencyConfiguration());
        modelBuilder.ApplyConfiguration(new CompanyCurrencyConfiguration());
        modelBuilder.ApplyConfiguration(new SubdivisionConfiguration());
        modelBuilder.ApplyConfiguration(new CityConfiguration());
        modelBuilder.ApplyConfiguration(new LocalityConfiguration());
        modelBuilder.ApplyConfiguration(new AddressConfiguration());
        modelBuilder.ApplyConfiguration(new ProductSupplierConfiguration());

        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("Products");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Sku).IsRequired();
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.Barcode).HasMaxLength(64);
            entity.Property(e => e.IsStackable);
            entity.Property(e => e.LengthCm).HasColumnType("numeric(18,3)");
            entity.Property(e => e.WidthCm).HasColumnType("numeric(18,3)");
            entity.Property(e => e.WeightKg).HasColumnType("numeric(18,3)");
            entity.Property(e => e.StorageType).HasMaxLength(64);
            entity.Property(e => e.UnitOfMeasureId).IsRequired();
            entity.Property(e => e.IsSellable).HasDefaultValue(true);
            entity.Property(e => e.IsPurchasable).HasDefaultValue(true);
            entity.HasIndex(e => new { e.CompanyId, e.Sku }).IsUnique();
            entity.HasIndex(e => new { e.CompanyId, e.Barcode }).IsUnique();
            entity.HasOne(e => e.UnitOfMeasure)
                .WithMany()
                .HasForeignKey(e => e.UnitOfMeasureId)
                .OnDelete(DeleteBehavior.Restrict);
            ConfigureCompanyReference(entity);
        });

        modelBuilder.Entity<UnitOfMeasure>(entity =>
        {
            entity.ToTable("UnitOfMeasures", "masterdata");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CanonicalCode).IsRequired();
            entity.Property(e => e.DisplayCode).IsRequired();
            entity.Property(e => e.Dimension).HasConversion<string>().IsRequired();
            entity.Property(e => e.IsBaseUnit).IsRequired();
            entity.Property(e => e.FactorToBase).HasPrecision(18, 8);
            entity.Property(e => e.PrecisionScale).IsRequired();
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.HasIndex(e => e.CanonicalCode).IsUnique();
            entity.HasMany(e => e.Translations)
                .WithOne(t => t.UnitOfMeasure)
                .HasForeignKey(t => t.UnitOfMeasureId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(e => e.ExternalMappings)
                .WithOne(m => m.UnitOfMeasure)
                .HasForeignKey(m => m.UnitOfMeasureId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.Navigation(e => e.Translations).UsePropertyAccessMode(PropertyAccessMode.Field);
            entity.Navigation(e => e.ExternalMappings).UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<UnitOfMeasureTranslation>(entity =>
        {
            entity.ToTable("UnitOfMeasureTranslations", "masterdata");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Culture).IsRequired();
            entity.Property(e => e.Name).IsRequired();
            entity.HasIndex(e => new { e.UnitOfMeasureId, e.Culture }).IsUnique();
        });

        modelBuilder.Entity<UnitOfMeasureExternalMapping>(entity =>
        {
            entity.ToTable("UnitOfMeasureExternalMappings", "masterdata");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Scheme).HasConversion<string>().IsRequired();
            entity.Property(e => e.Code).IsRequired();
            entity.HasIndex(e => new { e.Scheme, e.Code }).IsUnique();
        });

        modelBuilder.Entity<CompanyUnitOfMeasure>(entity =>
        {
            entity.ToTable("CompanyUnitsOfMeasure");
            entity.HasKey(e => new { e.CompanyId, e.UnitOfMeasureId });
            entity.Property(e => e.Dimension).HasConversion<string>().IsRequired();
            entity.Property(e => e.IsEnabled).HasDefaultValue(true);
            entity.Property(e => e.IsDefaultForDimension).HasDefaultValue(false);
            entity.HasIndex(e => new { e.CompanyId, e.IsEnabled });
            entity.HasIndex(e => new { e.CompanyId, e.Dimension })
                .IsUnique()
                .HasFilter("\"IsDefaultForDimension\" = TRUE");
            entity.HasIndex(e => e.UnitOfMeasureId);
            entity.HasOne(e => e.UnitOfMeasure)
                .WithMany()
                .HasForeignKey(e => e.UnitOfMeasureId)
                .OnDelete(DeleteBehavior.Restrict);
            ConfigureCompanyReference(entity);
        });

        modelBuilder.Entity<ProductUnitConversion>(entity =>
        {
            entity.ToTable("ProductUnitConversions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Factor).HasPrecision(18, 8);
            entity.Property(e => e.RoundingMode).HasConversion<string>();
            entity.HasIndex(e => new { e.ProductId, e.FromUnitOfMeasureId, e.ToUnitOfMeasureId }).IsUnique();
            entity.HasOne<Product>()
                .WithMany()
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.FromUnitOfMeasure)
                .WithMany()
                .HasForeignKey(e => e.FromUnitOfMeasureId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.ToUnitOfMeasure)
                .WithMany()
                .HasForeignKey(e => e.ToUnitOfMeasureId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.ToTable("Customers");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.Type).HasConversion<string>().HasMaxLength(32);
            ConfigureCompanyReference(entity);
        });

        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.ToTable("Suppliers");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.HasIndex(e => new { e.CompanyId, e.Name });
            entity.HasIndex(e => new { e.CompanyId, e.TaxId });
            entity.HasOne(e => e.Country)
                .WithMany()
                .HasForeignKey(e => e.CountryId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.DefaultCurrency)
                .WithMany()
                .HasForeignKey(e => e.DefaultCurrencyId)
                .OnDelete(DeleteBehavior.Restrict);
            ConfigureCompanyReference(entity);
        });

        modelBuilder.Entity<Warehouse>(entity =>
        {
            entity.ToTable("Warehouses");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).IsRequired();
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.Location).HasMaxLength(256);
            entity.HasIndex(e => new { e.CompanyId, e.Code }).IsUnique();
            ConfigureCompanyReference(entity);
        });

        modelBuilder.Entity<FeatureCatalog>(entity =>
        {
            entity.ToTable("FeatureCatalog", "masterdata");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PublicId).IsRequired();
            entity.Property(e => e.Code).IsRequired();
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.Description);
            entity.Property(e => e.IsActive).IsRequired();
            entity.Property(e => e.CreatedAt).HasColumnType("timestamp with time zone").IsRequired();
            entity.Property(e => e.UpdatedAt).HasColumnType("timestamp with time zone").IsRequired();
            entity.HasIndex(e => e.Code).IsUnique();
            entity.HasIndex(e => e.PublicId).IsUnique();
        });
    }

    private static void ConfigureCompanyFeatures(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CompanyFeature>(entity =>
        {
            entity.ToTable("CompanyFeatures", "public");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FeaturePublicId).IsRequired();
            entity.Property(e => e.IsActive).IsRequired();
            entity.Property(e => e.CreatedAt).HasColumnType("timestamp with time zone").IsRequired();
            entity.Property(e => e.UpdatedAt).HasColumnType("timestamp with time zone").IsRequired();
            entity.HasIndex(e => new { e.CompanyId, e.FeaturePublicId }).IsUnique();
            ConfigureCompanyReference(entity);
        });

        modelBuilder.Entity<CompanyFeatureAudit>(entity =>
        {
            entity.ToTable("CompanyFeatureAudits", "public");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FeaturePublicId).IsRequired();
            entity.Property(e => e.Action).HasConversion<string>().IsRequired();
            entity.Property(e => e.ChangedAt).HasColumnType("timestamp with time zone").IsRequired();
            entity.Property(e => e.ChangedByUserId).IsRequired();
            entity.Property(e => e.CorrelationId).HasColumnType("uuid");
            ConfigureCompanyReference(entity);
        });
    }

    private static void ConfigureSubscriptions(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Plan>(entity =>
        {
            entity.ToTable("SubscriptionPlans");
            entity.HasKey(plan => plan.Id);
            entity.Property(plan => plan.Code).IsRequired().HasMaxLength(64);
            entity.Property(plan => plan.Name).IsRequired().HasMaxLength(128);
            entity.Property(plan => plan.Description).HasMaxLength(512);
            entity.Property(plan => plan.PriceCurrency).IsRequired().HasMaxLength(8);
            entity.Property(plan => plan.BillingCycle).HasConversion<string>().IsRequired().HasMaxLength(32);
            entity.Property(plan => plan.CreatedAt).IsRequired();
            entity.HasIndex(plan => plan.Code).IsUnique();
            entity.HasMany(plan => plan.Features)
                .WithOne()
                .HasForeignKey(feature => feature.PlanId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PlanFeature>(entity =>
        {
            entity.ToTable("SubscriptionPlanFeatures");
            entity.HasKey(feature => feature.Id);
            entity.Property(feature => feature.FeatureKey).IsRequired().HasMaxLength(128);
            entity.HasIndex(feature => new { feature.PlanId, feature.FeatureKey }).IsUnique();
        });

        modelBuilder.Entity<Subscription>(entity =>
        {
            entity.ToTable("Subscriptions");
            entity.HasKey(subscription => subscription.Id);
            entity.Property(subscription => subscription.Status).HasConversion<string>().IsRequired().HasMaxLength(32);
            entity.Property(subscription => subscription.StartedAt).IsRequired();
            entity.HasIndex(subscription => new { subscription.CompanyId, subscription.Status });
            entity.HasOne(subscription => subscription.Plan)
                .WithMany()
                .HasForeignKey(subscription => subscription.PlanId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(subscription => subscription.CurrentBillingCycle)
                .WithOne(cycle => cycle.Subscription)
                .HasForeignKey<BillingCycle>(cycle => cycle.SubscriptionId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(subscription => subscription.Trial)
                .WithOne(trial => trial.Subscription)
                .HasForeignKey<Trial>(trial => trial.SubscriptionId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(subscription => subscription.SeatCount)
                .WithOne(seat => seat.Subscription)
                .HasForeignKey<SeatCount>(seat => seat.SubscriptionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<BillingCycle>(entity =>
        {
            entity.ToTable("SubscriptionBillingCycles");
            entity.HasKey(cycle => cycle.Id);
            entity.Property(cycle => cycle.CycleType).HasConversion<string>().IsRequired().HasMaxLength(32);
            entity.Property(cycle => cycle.Status).HasConversion<string>().IsRequired().HasMaxLength(32);
            entity.Property(cycle => cycle.StartsAt).IsRequired();
            entity.Property(cycle => cycle.EndsAt).IsRequired();
            entity.HasIndex(cycle => new { cycle.SubscriptionId, cycle.StartsAt });
        });

        modelBuilder.Entity<Trial>(entity =>
        {
            entity.ToTable("SubscriptionTrials");
            entity.HasKey(trial => trial.Id);
            entity.Property(trial => trial.Status).HasConversion<string>().IsRequired().HasMaxLength(32);
            entity.Property(trial => trial.StartsAt).IsRequired();
            entity.Property(trial => trial.EndsAt).IsRequired();
        });

        modelBuilder.Entity<SeatCount>(entity =>
        {
            entity.ToTable("SubscriptionSeatCounts");
            entity.HasKey(seat => seat.Id);
            entity.Property(seat => seat.MaxSeats).IsRequired();
            entity.Property(seat => seat.UsedSeats).IsRequired();
            entity.Property(seat => seat.UpdatedAt).IsRequired();
        });

        modelBuilder.Entity<SubscriptionUsage>(entity =>
        {
            entity.ToTable("SubscriptionUsage");
            entity.HasKey(usage => usage.Id);
            entity.Property(usage => usage.Metric).IsRequired().HasMaxLength(64);
            entity.Property(usage => usage.Quantity).IsRequired();
            entity.Property(usage => usage.RecordedAt).IsRequired();
            entity.HasIndex(usage => new { usage.SubscriptionId, usage.RecordedAt });
        });
    }

    private static void ConfigurePricing(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PriceList>(entity =>
        {
            entity.ToTable("PriceLists");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired();
            entity.HasIndex(e => new { e.CompanyId, e.Name }).IsUnique();
            ConfigureCompanyReference(entity);
        });

        modelBuilder.Entity<PriceListItem>(entity =>
        {
            entity.ToTable("PriceListItems");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.CompanyId, e.PriceListId, e.ProductId }).IsUnique();
            ConfigureCompanyReference(entity);
            entity.OwnsOne(e => e.UnitPrice, owned =>
            {
                owned.Property(p => p.Amount).HasColumnName("UnitPriceAmount").HasPrecision(18, 2);
                owned.Property(p => p.Currency).HasColumnName("UnitPriceCurrency").HasMaxLength(3);
            });
        });

        modelBuilder.Entity<PricingPolicy>(entity =>
        {
            entity.ToTable("PricingPolicies");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.MarginPercent).HasPrecision(8, 2);
            entity.Property(e => e.RoundingMode).HasConversion<string>().HasMaxLength(32);
            entity.HasIndex(e => e.CompanyId).IsUnique();
            ConfigureCompanyReference(entity);
        });
    }

    private static void ConfigureInventory(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Stock>(entity =>
        {
            entity.ToTable("Stocks");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.CompanyId, e.ProductId, e.WarehouseId }).IsUnique();
            entity.Property(e => e.OnHandQuantity).HasPrecision(18, 2);
            entity.Property<uint>("xmin").IsRowVersion().HasColumnName("xmin");
            ConfigureCompanyReference(entity);
        });

        modelBuilder.Entity<InventoryMovement>(entity =>
        {
            entity.ToTable("InventoryMovements");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ReferenceType).IsRequired();
            entity.Property(e => e.ReferenceId).IsRequired();
            entity.Property(e => e.Source).HasMaxLength(64);
            entity.Property(e => e.SourceId).HasMaxLength(128);
            entity.Property(e => e.Quantity).HasPrecision(18, 2);
            entity.HasIndex(e => new { e.CompanyId, e.MovementType, e.CreatedAt });
            entity.HasIndex(e => new { e.CompanyId, e.ProductId, e.CreatedAt });
            entity.HasIndex(e => new { e.CompanyId, e.Source, e.SourceId }).IsUnique();
            ConfigureCompanyReference(entity);
        });
    }

    private static void ConfigureWmsTable<TEntity>(EntityTypeBuilder<TEntity> entity, string tableName)
        where TEntity : class
    {
        entity.ToTable(tableName, WmsSchema);
    }

    private static void EnsureWmsSchemaMappings(ModelBuilder modelBuilder)
    {
        var invalidEntities = modelBuilder.Model.GetEntityTypes()
            .Where(entityType => entityType.ClrType.Namespace == "ERP.Modules.Wms.Domain"
                && !entityType.IsOwned())
            .Where(entityType => !string.Equals(entityType.GetSchema(), WmsSchema, StringComparison.Ordinal))
            .Select(entityType => $"{entityType.ClrType.Name} -> {entityType.GetSchema() ?? "<default>"}")
            .ToList();

        if (invalidEntities.Count == 0)
        {
            return;
        }

        throw new InvalidOperationException(
            $"WMS entities must map to schema '{WmsSchema}'. Invalid mappings: {string.Join(", ", invalidEntities)}");
    }

    private static void ConfigurePurchasing(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PurchaseOrder>(entity =>
        {
            entity.ToTable("PurchaseOrders");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Currency).IsRequired();
            entity.HasMany(e => e.Lines)
                .WithOne()
                .HasForeignKey(l => l.PurchaseOrderId)
                .OnDelete(DeleteBehavior.Cascade);
            ConfigureCompanyReference(entity);
        });

        modelBuilder.Entity<PurchaseOrderLine>(entity =>
        {
            entity.ToTable("PurchaseOrderLines");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.OrderedQty).HasPrecision(18, 2);
            entity.OwnsOne(e => e.UnitPriceRef, (OwnedNavigationBuilder<PurchaseOrderLine, Money> owned) =>
            {
                owned.Property(p => p.Amount).HasColumnName("UnitPriceAmount").HasPrecision(18, 2);
                owned.Property(p => p.Currency).HasColumnName("UnitPriceCurrency").HasMaxLength(3);
            });
            entity.OwnsOne(e => e.TaxAmount, owned =>
            {
                owned.Property(p => p.Amount).HasColumnName("TaxAmount").HasPrecision(18, 2);
                owned.Property(p => p.Currency).HasColumnName("TaxCurrency").HasMaxLength(3);
            });
            entity.OwnsOne(e => e.NetAmount, owned =>
            {
                owned.Property(p => p.Amount).HasColumnName("NetAmount").HasPrecision(18, 2);
                owned.Property(p => p.Currency).HasColumnName("NetCurrency").HasMaxLength(3);
            });
            entity.OwnsOne(e => e.TotalAmount, owned =>
            {
                owned.Property(p => p.Amount).HasColumnName("TotalAmount").HasPrecision(18, 2);
                owned.Property(p => p.Currency).HasColumnName("TotalCurrency").HasMaxLength(3);
            });
            ConfigureCompanyReference(entity);
        });

        modelBuilder.Entity<PurchaseSuggestion>(entity =>
        {
            entity.ToTable("PurchaseSuggestions");
            entity.HasKey(e => e.Id);
            entity.HasMany(e => e.Lines)
                .WithOne()
                .HasForeignKey(l => l.PurchaseSuggestionId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.CompanyId, e.Status, e.CreatedAt });
            ConfigureCompanyReference(entity);
        });

        modelBuilder.Entity<PurchaseSuggestionLine>(entity =>
        {
            entity.ToTable("PurchaseSuggestionLines");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.QtySuggested).HasPrecision(18, 2);
            entity.HasIndex(e => new { e.CompanyId, e.PurchaseSuggestionId });
            ConfigureCompanyReference(entity);
        });

        modelBuilder.Entity<GoodsReceipt>(entity =>
        {
            entity.ToTable("GoodsReceipts");
            entity.HasKey(e => e.Id);
            entity.HasMany(e => e.Lines)
                .WithOne()
                .HasForeignKey(l => l.GoodsReceiptId)
                .OnDelete(DeleteBehavior.Cascade);
            ConfigureCompanyReference(entity);
        });

        modelBuilder.Entity<GoodsReceiptLine>(entity =>
        {
            entity.ToTable("GoodsReceiptLines");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ReceivedQty).HasPrecision(18, 2);
            ConfigureCompanyReference(entity);
        });
    }

    private static void ConfigureSales(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SalesDocument>(entity =>
        {
            entity.ToTable("SalesDocuments");
            entity.HasKey(e => e.Id);
            entity.HasMany(e => e.Lines)
                .WithOne()
                .HasForeignKey(l => l.SalesDocumentId)
                .OnDelete(DeleteBehavior.Cascade);
            ConfigureCompanyReference(entity);
        });

        modelBuilder.Entity<SalesDocumentLine>(entity =>
        {
            entity.ToTable("SalesDocumentLines");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Qty).HasPrecision(18, 2);
            entity.OwnsOne(e => e.UnitPrice, (OwnedNavigationBuilder<SalesDocumentLine, Money> owned) =>
            {
                owned.Property(p => p.Amount).HasColumnName("UnitPriceAmount").HasPrecision(18, 2);
                owned.Property(p => p.Currency).HasColumnName("UnitPriceCurrency").HasMaxLength(3);
            });
            entity.OwnsOne(e => e.NetAmount, owned =>
            {
                owned.Property(p => p.Amount).HasColumnName("NetAmount").HasPrecision(18, 2);
                owned.Property(p => p.Currency).HasColumnName("NetCurrency").HasMaxLength(3);
            });
            entity.OwnsOne(e => e.TaxAmount, owned =>
            {
                owned.Property(p => p.Amount).HasColumnName("TaxAmount").HasPrecision(18, 2);
                owned.Property(p => p.Currency).HasColumnName("TaxCurrency").HasMaxLength(3);
            });
            entity.OwnsOne(e => e.TotalAmount, owned =>
            {
                owned.Property(p => p.Amount).HasColumnName("TotalAmount").HasPrecision(18, 2);
                owned.Property(p => p.Currency).HasColumnName("TotalCurrency").HasMaxLength(3);
            });
            ConfigureCompanyReference(entity);
        });

        modelBuilder.Entity<SalesQuote>(entity =>
        {
            entity.ToTable("SalesQuotes", SalesSchema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PublicId).IsRequired();
            entity.Property(e => e.QuoteNumber).IsRequired().HasMaxLength(64);
            entity.Property(e => e.Status).HasConversion<int>().IsRequired();
            entity.Property(e => e.CustomerName).IsRequired();
            entity.Property(e => e.Notes);
            entity.Property(e => e.CurrencyCode).HasMaxLength(3);
            entity.Property(e => e.Subtotal).HasPrecision(18, 2);
            entity.Property(e => e.TaxTotal).HasPrecision(18, 2);
            entity.Property(e => e.Total).HasPrecision(18, 2);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.CreatedByUserId).IsRequired();
            entity.Property(e => e.UpdatedByUserId).IsRequired();
            entity.HasMany(e => e.Lines)
                .WithOne()
                .HasForeignKey(l => l.QuoteId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.CompanyId, e.PublicId }).IsUnique();
            entity.HasIndex(e => new { e.CompanyId, e.Status });
            ConfigureCompanyReference(entity);
        });

        modelBuilder.Entity<SalesQuoteLine>(entity =>
        {
            entity.ToTable("SalesQuoteLines", SalesSchema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.LineNumber).IsRequired();
            entity.Property(e => e.Description).IsRequired();
            entity.Property(e => e.Quantity).HasPrecision(18, 4);
            entity.Property(e => e.UnitPrice).HasPrecision(18, 4);
            entity.Property(e => e.LineTotal).HasPrecision(18, 2);
            entity.Property(e => e.TaxRate).HasPrecision(5, 2);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.HasIndex(e => e.QuoteId);
            ConfigureCompanyReference(entity);
        });
    }

    private static void ConfigureBilling(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.ToTable("Invoices");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Number).IsRequired();
            entity.Property(e => e.Status).IsRequired();
            entity.Property(e => e.Currency).HasMaxLength(3);
            entity.HasIndex(e => new { e.CompanyId, e.Number }).IsUnique();
            entity.HasMany(e => e.Lines)
                .WithOne()
                .HasForeignKey(l => l.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);
            ConfigureCompanyReference(entity);
        });

        modelBuilder.Entity<InvoiceLine>(entity =>
        {
            entity.ToTable("InvoiceLines");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Qty).HasPrecision(18, 2);
            ConfigureCompanyReference(entity);
            entity.OwnsOne(e => e.UnitPrice, owned =>
            {
                owned.Property(p => p.Amount).HasColumnName("UnitPriceAmount").HasPrecision(18, 2);
                owned.Property(p => p.Currency).HasColumnName("UnitPriceCurrency").HasMaxLength(3);
            });
        });

        modelBuilder.Entity<CreditNote>(entity =>
        {
            entity.ToTable("CreditNotes");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Number).IsRequired();
            entity.Property(e => e.Reason).IsRequired();
            entity.Property(e => e.Status).IsRequired();
            entity.HasIndex(e => new { e.CompanyId, e.Number }).IsUnique();
            entity.OwnsOne(e => e.Amount, owned =>
            {
                owned.Property(p => p.Amount).HasColumnName("Amount").HasPrecision(18, 2);
                owned.Property(p => p.Currency).HasColumnName("Currency").HasMaxLength(3);
            });
            ConfigureCompanyReference(entity);
        });

        modelBuilder.Entity<DebitNote>(entity =>
        {
            entity.ToTable("DebitNotes");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Number).IsRequired();
            entity.Property(e => e.Status).IsRequired();
            entity.HasIndex(e => new { e.CompanyId, e.Number }).IsUnique();
            entity.OwnsOne(e => e.Amount, owned =>
            {
                owned.Property(p => p.Amount).HasColumnName("Amount").HasPrecision(18, 2);
                owned.Property(p => p.Currency).HasColumnName("Currency").HasMaxLength(3);
            });
            ConfigureCompanyReference(entity);
        });
    }

    private static void ConfigureTax(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Tax>(entity =>
        {
            entity.ToTable("Taxes");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).IsRequired();
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.Rate).HasPrecision(8, 4);
            entity.HasIndex(e => new { e.CompanyId, e.Code }).IsUnique();
            ConfigureCompanyReference(entity);
        });

        modelBuilder.Entity<TaxGroup>(entity =>
        {
            entity.ToTable("TaxGroups");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired();
            entity.HasMany(e => e.Rules)
                .WithOne()
                .HasForeignKey(r => r.TaxGroupId)
                .OnDelete(DeleteBehavior.Cascade);
            ConfigureCompanyReference(entity);
        });

        modelBuilder.Entity<TaxRule>(entity =>
        {
            entity.ToTable("TaxRules");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Sequence).IsRequired();
            entity.Property(e => e.Rate).HasPrecision(8, 4);
            entity.Property(e => e.IsCompound).IsRequired();
            entity.HasOne<Tax>()
                .WithMany()
                .HasForeignKey(r => r.TaxId)
                .OnDelete(DeleteBehavior.Restrict);
            ConfigureCompanyReference(entity);
        });
    }

    private static void ConfigureCash(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BankAccount>(entity =>
        {
            entity.ToTable("BankAccounts");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.BankName).IsRequired();
            entity.Property(e => e.AccountNumber).IsRequired();
            entity.Property(e => e.Currency).IsRequired();
            ConfigureCompanyReference(entity);
        });

        modelBuilder.Entity<BankStatement>(entity =>
        {
            entity.ToTable("BankStatements");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.BankAccountName).IsRequired(false);
            entity.Property(e => e.StartingBalance).HasPrecision(18, 2);
            entity.Property(e => e.EndingBalance).HasPrecision(18, 2);
            entity.HasMany(e => e.Transactions)
                .WithOne()
                .HasForeignKey(t => t.BankStatementId)
                .OnDelete(DeleteBehavior.Cascade);
            ConfigureCompanyReference(entity);
        });

        modelBuilder.Entity<BankTransaction>(entity =>
        {
            entity.ToTable("BankTransactions");
            entity.HasKey(e => e.Id);
            entity.OwnsOne(e => e.Amount, owned =>
            {
                owned.Property(p => p.Amount).HasColumnName("Amount").HasPrecision(18, 2);
                owned.Property(p => p.Currency).HasColumnName("Currency").HasMaxLength(3);
            });
            entity.Property(e => e.Description).HasMaxLength(512);
            ConfigureCompanyReference(entity);
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.ToTable("Payments");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PaidAt).IsRequired();
            entity.Property(e => e.AppliedAmount).HasPrecision(18, 2);
            entity.OwnsOne(e => e.Amount, owned =>
            {
                owned.Property(p => p.Amount).HasColumnName("Amount").HasPrecision(18, 2);
                owned.Property(p => p.Currency).HasColumnName("Currency").HasMaxLength(3);
            });
            ConfigureCompanyReference(entity);
        });

        modelBuilder.Entity<Receipt>(entity =>
        {
            entity.ToTable("Receipts");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ReceivedAt).IsRequired();
            entity.Property(e => e.AppliedAmount).HasPrecision(18, 2);
            entity.OwnsOne(e => e.Amount, owned =>
            {
                owned.Property(p => p.Amount).HasColumnName("Amount").HasPrecision(18, 2);
                owned.Property(p => p.Currency).HasColumnName("Currency").HasMaxLength(3);
            });
            ConfigureCompanyReference(entity);
        });
    }

    private static void ConfigureDocuments(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Document>(entity =>
        {
            entity.ToTable("Documents", DocumentSchema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CompanyPublicId).IsRequired();
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Ignore(e => e.Tags);
            entity.HasMany(e => e.Versions)
                .WithOne()
                .HasForeignKey("DocumentId")
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(e => e.Links)
                .WithOne()
                .HasForeignKey("DocumentId")
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.CompanyPublicId, e.CreatedAt });
        });

        modelBuilder.Entity<DocumentVersion>(entity =>
        {
            entity.ToTable("DocumentVersions", DocumentSchema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CompanyPublicId).IsRequired();
            entity.Property(e => e.FileName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.ContentType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Size).IsRequired();
            entity.Property(e => e.Label).HasMaxLength(200);
            entity.Property(e => e.UploadedAt).IsRequired();
            entity.Property<long>("DocumentId");
            entity.HasIndex(e => new { e.CompanyPublicId, e.UploadedAt });
            entity.HasIndex("DocumentId");
        });

        modelBuilder.Entity<DocumentLink>(entity =>
        {
            entity.ToTable("DocumentLinks", DocumentSchema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CompanyPublicId).IsRequired();
            entity.Property(e => e.Url).IsRequired().HasMaxLength(2000);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property<long>("DocumentId");
            entity.HasIndex(e => new { e.CompanyPublicId, e.CreatedAt });
            entity.HasIndex("DocumentId");
        });
    }

    private static void ConfigureUsers(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserProfile>(entity =>
        {
            entity.ToTable("UserProfiles", "iam");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.Email).IsRequired();
            entity.Property(e => e.FirstName).IsRequired();
            entity.Property(e => e.LastName).IsRequired();
            entity.HasIndex(e => e.UserId).IsUnique();
            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureIntegrations(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OutboxMessage>(entity =>
        {
            entity.ToTable("OutboxMessages");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Type).IsRequired();
            entity.Property(e => e.PayloadJson).IsRequired();
            entity.HasIndex(e => e.ProcessedAt);
            entity.HasIndex(e => e.OccurredAt)
                .HasDatabaseName("IX_OutboxMessages_OccurredAt_Desc")
                .IsDescending(true);
            entity.HasIndex(e => new { e.Type, e.OccurredAt })
                .HasDatabaseName("IX_OutboxMessages_Type_OccurredAt_Desc")
                .IsDescending(false, true);
        });
    }

    private static void ConfigureIdentity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users", "iam");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).IsRequired();
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.PasswordSalt).IsRequired();
            entity.HasIndex(e => e.Email).IsUnique();
        });

        modelBuilder.Entity<UserSession>(entity =>
        {
            entity.ToTable("UserSessions", "iam");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SessionId).IsRequired();
            entity.Property(e => e.DeviceInfo).IsRequired().HasMaxLength(512);
            entity.Property(e => e.RefreshTokenHash).IsRequired();
            entity.Property(e => e.ReplacedByTokenHash);
            entity.Property(e => e.ReplacedAtUtc);
            entity.Property(e => e.ExpiresAt).IsRequired();
            entity.Property(e => e.LastSeenAt).IsRequired();
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.SessionId);
            entity.HasIndex(e => e.RefreshTokenHash).IsUnique();
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("Roles", "iam");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(128);
            entity.Property(e => e.Description).HasMaxLength(512);
            entity.Property(e => e.RequiredFeatureKey).HasMaxLength(128);
            entity.Property(e => e.IsSystem).HasDefaultValue(true);
            entity.HasIndex(e => e.Name);
        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.ToTable("Permissions", "iam");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.Code).IsRequired().HasMaxLength(128);
            entity.Property(e => e.Description).HasMaxLength(512);
            entity.Property(e => e.ScopeType)
                .IsRequired()
                .HasConversion<short>()
                .HasDefaultValue(RoleAssignmentScopeType.Company)
                .HasSentinel((RoleAssignmentScopeType)0);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.HasIndex(e => e.Name).IsUnique();
            entity.HasIndex(e => e.Code).IsUnique();
        });



        modelBuilder.Entity<CompanyUser>(entity =>
        {
            entity.ToTable("CompanyUsers", "iam");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Status)
                .IsRequired()
                .HasDefaultValue(CompanyUserStatus.Active)
                .HasSentinel((CompanyUserStatus)0);
            entity.Property(e => e.CompanyPublicId).IsRequired();
            entity.HasIndex(e => new { e.CompanyPublicId, e.UserId }).IsUnique();
            entity.HasOne(e => e.User)
                .WithMany(e => e.CompanyUsers)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });



        modelBuilder.Entity<OrganizationMember>(entity =>
        {
            entity.ToTable("OrganizationMembers", "iam");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.OrganizationId).IsRequired();
            entity.Property(e => e.Role).HasConversion<string>().IsRequired();
            entity.HasIndex(e => new { e.OrganizationId, e.UserId }).IsUnique();
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<Organization>()
                .WithMany()
                .HasForeignKey(e => e.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<CompanyLink>(entity =>
        {
            entity.ToTable("CompanyLinks", "iam");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.OrganizationId).IsRequired();
            entity.Property(e => e.CompanyPublicId).IsRequired();
            entity.Property(e => e.AccessType).HasConversion<string>().IsRequired();
            entity.HasIndex(e => new { e.OrganizationId, e.CompanyPublicId }).IsUnique();
            entity.HasOne<Organization>()
                .WithMany()
                .HasForeignKey(e => e.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.ToTable("RolePermissions", "iam");
            entity.HasKey(e => new { e.RoleId, e.PermissionId });
            entity.HasOne(e => e.Role)
                .WithMany(e => e.RolePermissions)
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Permission)
                .WithMany(e => e.RolePermissions)
                .HasForeignKey(e => e.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RoleAssignment>(entity =>
        {
            entity.ToTable("RoleAssignments", "iam");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn();
            
            entity.Property(e => e.UserId)
                .IsRequired();
            
entity.Property(e => e.RoleId)
                .IsRequired();
            
            entity.Property(e => e.OrganizationId);
            
            entity.Property(e => e.CompanyPublicId);
            
            entity.Property(e => e.Status)
                .IsRequired()
                .HasConversion<short>()
                .HasDefaultValue(RoleAssignmentStatus.Active)
                .HasSentinel((RoleAssignmentStatus)0);
            
            entity.Property(e => e.CreatedAtUtc)
                .IsRequired()
                .HasDefaultValueSql("now()");
            
            entity.Property(e => e.UpdatedAtUtc)
                .IsRequired()
                .HasDefaultValueSql("now()");

// Performance indexes
            entity.HasIndex(e => new { e.UserId, e.Status });
            entity.HasIndex(e => new { e.CompanyPublicId, e.Status });
            entity.HasIndex(e => new { e.OrganizationId, e.Status });
            entity.HasIndex(e => new { e.UserId, e.CompanyPublicId, e.Status });
            entity.HasIndex(e => e.RoleId);

            // Foreign keys within iam schema only
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.Role)
                .WithMany()
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<UserAuditLog>(entity =>
        {
            entity.ToTable("UserAuditLog", "iam");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Action).IsRequired();
            entity.Property(e => e.EntityName).IsRequired();
            entity.Property(e => e.EntityId).IsRequired();
            entity.Property(e => e.ChangesJson).IsRequired();
            entity.Property(e => e.Source).IsRequired();
            entity.HasIndex(e => e.OccurredAt);
        });

        modelBuilder.ApplyConfiguration(new PasswordResetTokenConfiguration());
        modelBuilder.ApplyConfiguration(new UserPasswordHistoryConfiguration());
    }

    private static void ConfigureAudit(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("AuditLogs");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Action).IsRequired();
            entity.Property(e => e.EntityName).IsRequired();
            entity.Property(e => e.EntityId).IsRequired();
            entity.Property(e => e.ChangesJson).IsRequired();
            entity.Property(e => e.Source).IsRequired();
            entity.HasIndex(e => e.OccurredAt);
        });
    }

    private static void ConfigureDocumentSearch(ModelBuilder modelBuilder, bool isNpgsql)
    {
        modelBuilder.Entity<DocumentSearchEntry>(entity =>
        {
            entity.ToTable("DocumentSearchEntries");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.DocumentType).IsRequired().HasMaxLength(200);
            entity.Property(e => e.DocumentId).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Title).HasMaxLength(300);
            entity.Property(e => e.Content).IsRequired();

            if (isNpgsql)
            {
                entity.Property(e => e.Metadata)
                    .HasColumnType("jsonb")
                    .HasDefaultValueSql("'{}'::jsonb")
                    .IsRequired();

                entity.Property<NpgsqlTsVector>("SearchVector")
                    .HasComputedColumnSql(
                        "to_tsvector('simple', coalesce(\"Title\", '') || ' ' || coalesce(\"Content\", ''))",
                        stored: true);
            }
            else
            {
                entity.Property(e => e.Metadata)
                    .HasConversion(
                        metadata => JsonSerializer.Serialize(metadata ?? new Dictionary<string, string>(), DocumentSearchJsonOptions),
                        json => JsonSerializer.Deserialize<Dictionary<string, string>>(json, DocumentSearchJsonOptions) ?? new Dictionary<string, string>())
                    .HasColumnType("TEXT")
                    .IsRequired();
            }

            entity.HasIndex(e => new { e.CompanyId, e.DocumentType, e.DocumentId }).IsUnique();
            if (isNpgsql)
            {
                entity.HasIndex(e => e.Metadata).HasMethod("GIN");
                entity.HasIndex("SearchVector").HasMethod("GIN");
            }
        });
    }
}

