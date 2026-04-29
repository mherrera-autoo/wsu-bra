using System.Text;
using ERP.Modules.Accounting.Application.Repositories;
using ERP.Modules.FixedAssets.Application.Repositories;
using ERP.Modules.FixedAssets.Application.Services;
using ERP.Modules.FixedAssets.Infrastructure.Repositories;
using ERP.Modules.Billing.Application.Repositories;
using ERP.Modules.Billing.Application.Services;
using ERP.Modules.Finance.Application.Repositories;
using ERP.Modules.Finance.Application.Services;
using ERP.Modules.Cash.Application.Repositories;
using ERP.Modules.Cash.Application.Services;
using ERP.Modules.Sales.Application.Handlers;
using ERP.Api.Authentication;
using ERP.Api.Authorization;
using ERP.Api.Filters;
using ERP.Api.Services;
using ERP.Modules.Identity.Application.Repositories;
using ERP.Modules.Identity.Application.Security;
using ERP.Modules.Identity.Application.Services;
using ERP.Modules.Identity.Application.Options;
using ERP.Modules.Identity.Application.Commands;
using ERP.Modules.Integrations.Contracts;
using ERP.Modules.Tax.Contracts;
using ERP.Modules.Tax.Application.Services;
using ERP.Modules.Inventory.Application.Repositories;
using ERP.Modules.Inventory.Application.Services;
using ERP.Modules.Users.Application.Repositories;
using ERP.Modules.Users.Application.Services;
using ERP.Modules.Rfid.Application.Repositories;
using ERP.Modules.Rfid.Application.Services;
using ERP.Modules.Wsu.Application.Repositories;
using ERP.Modules.Wsu.Application.Services;
using ERP.Modules.Accounting.Application.Services;
using ERP.Modules.Accounting.Application.Reports;
using ERP.Modules.MasterData.Contracts;
using ERP.Modules.MasterData.Application.Services;
using ERP.Modules.PharmaceuticalRegulatedInventory.Application.Handlers;
using ERP.Modules.PharmaceuticalRegulatedInventory.Application.Repositories;
using ERP.Modules.PharmaceuticalRegulatedInventory.Application.Reporting;
using ERP.Modules.PharmaceuticalRegulatedInventory.Application.Services;
using ERP.Modules.Wms.Application.Repositories;
using ERP.Modules.Wms.Application.Services;
using ERP.Modules.Purchasing.Application.Repositories;
using ERP.Modules.Purchasing.Application.Services;
using ERP.Modules.Purchasing.Contracts;
using ERP.Modules.Pricing.Application.Repositories;
using ERP.Modules.Pricing.Application.Services;
using ERP.Modules.Sales.Application.Repositories;
using ERP.Modules.Sales.Application.Services;
using ERP.Modules.Sales.Contracts;
using ERP.Modules.Tax.Application.Repositories;
using ERP.Modules.Subscriptions.Application.Repositories;
using ERP.Modules.Subscriptions.Application.Services;
using ERP.Persistence.Interceptors;
using ERP.Persistence.Reports;
using ERP.Persistence.Repositories;
using ERP.Persistence.Services;
using PasswordResetTokenRepository = ERP.Persistence.Repositories.PasswordResetTokenRepository;
using UserPasswordHistoryRepository = ERP.Persistence.Repositories.UserPasswordHistoryRepository;
using ERP.Api.Middleware;
using ERP.Api.Swagger;
using ERP.Api.Configuration;
using ERP.Shared.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using AccountingAccountsPayableRepository = ERP.Modules.Accounting.Application.Repositories.IAccountsPayableRepository;
using AccountingAccountsReceivableRepository = ERP.Modules.Accounting.Application.Repositories.IAccountsReceivableRepository;
using FinanceAccountsPayableRepository = ERP.Modules.Finance.Application.Repositories.IAccountsPayableRepository;
using FinanceAccountsReceivableRepository = ERP.Modules.Finance.Application.Repositories.IAccountsReceivableRepository;
using ApiJwtOptions = ERP.Api.Authentication.JwtOptions;
using IdentityJwtOptions = ERP.Modules.Identity.Application.Services.JwtOptions;
using ERP.Persistence;
using ERP.Modules.MasterData.Application.Repositories;
using ERP.Modules.Integrations.Infrastructure;
using ERP.Notifications;
using ERP.Documents.Application.Handlers;
using ERP.Documents.Application.Repositories;
using ERP.Documents.Infrastructure.Repositories;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);
var isProduction = builder.Environment.IsProduction();
const string devJwtSigningKey = "dev-secret-change-me-that-is-long-enough-for-hs256-algorithm";

// Configure hot reload explicitly for email settings
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Services.AddControllers(options =>
{
    options.Filters.Add<FeatureNotEnabledExceptionFilter>();
    options.Filters.Add<CompanyIdInputFilter>();
});


builder.Services.AddHttpContextAccessor();
builder.Services.AddEndpointsApiExplorer();
var swaggerTags = SwaggerTagConfigLoader.Load(builder.Environment);
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "JWT Authorization header using the Bearer scheme."
    });
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("bearer", document)] = []
    });
    options.OperationFilter<DevSeedHeaderOperationFilter>();
    options.OperationFilter<GlobalSeedHeaderOperationFilter>();
    options.OperationFilter<GlobalErrorResponsesOperationFilter>();
    options.DocumentFilter<SwaggerTagDocumentFilter>(swaggerTags);
    options.CustomSchemaIds(type => type.FullName?.Replace('+', '.') ?? type.Name);
});

builder.Services.AddScoped<ICurrentUserProvider, ERP.Api.Authentication.HttpContextCurrentUserProvider>();
builder.Services.AddScoped<AuditSaveChangesInterceptor>();
builder.Services.AddDbContext<ErpDbContext>((sp, options) =>
{
    if (builder.Environment.IsEnvironment("Testing"))
    {
        options.UseInMemoryDatabase("erp-tests");
        options.ConfigureWarnings(warnings =>
            warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning));
    }
    else
    {
        // 1) preferred key for containerized deployments
        var connectionString = builder.Configuration.GetConnectionString("Default");

        // 2) backward-compatible key used by existing environments
        if (string.IsNullOrWhiteSpace(connectionString))
            connectionString = builder.Configuration.GetConnectionString("ErpDatabase");

        // 3) env var directa (alineado con ErpDbContextFactory)
        if (string.IsNullOrWhiteSpace(connectionString))
            connectionString = Environment.GetEnvironmentVariable("ERP_DB");

        // 4) fallback local (non-production only)
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            if (isProduction)
                throw new InvalidOperationException("Database connection string is not configured. Set ConnectionStrings:Default, ConnectionStrings:ErpDatabase, or ERP_DB.");

            connectionString = "Host=localhost;Port=5432;Database=erp;Username=erp;Password=erp";
        }

        options.UseNpgsql(connectionString);
    }
    options.AddInterceptors(sp.GetRequiredService<AuditSaveChangesInterceptor>());
});

var jwtOptions = builder.Configuration.GetSection("Jwt").Get<ApiJwtOptions>() ?? new ApiJwtOptions();
if (string.IsNullOrWhiteSpace(jwtOptions.SigningKey))
{
    var signingKeyFromEnvironment = builder.Configuration["JWT_SIGNING_KEY"];
    jwtOptions.SigningKey = isProduction
        ? signingKeyFromEnvironment ?? string.Empty
        : signingKeyFromEnvironment ?? devJwtSigningKey;
}

if (isProduction && string.IsNullOrWhiteSpace(jwtOptions.SigningKey))
{
    throw new InvalidOperationException("JWT signing key is not configured. Set Jwt:SigningKey or JWT_SIGNING_KEY.");
}

var authHttpOptions = builder.Configuration.Get<AuthHttpOptions>() ?? new AuthHttpOptions();
if (authHttpOptions.AllowedOrigins.Length == 0)
{
    var originsEnv = builder.Configuration["ALLOWED_ORIGINS"];
    authHttpOptions.AllowedOrigins = string.IsNullOrWhiteSpace(originsEnv)
        ? ["http://localhost:5173"]
        : originsEnv.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
}

builder.Services.Configure<AuthHttpOptions>(options =>
{
    options.AllowedOrigins = authHttpOptions.AllowedOrigins;
    options.CookieSecure = authHttpOptions.CookieSecure;
    options.SameSite = authHttpOptions.SameSite;
});

builder.Services.Configure<ApiJwtOptions>(builder.Configuration.GetSection("Jwt"));
builder.Services.Configure<IdentityJwtOptions>(builder.Configuration.GetSection("Jwt"));
builder.Services.Configure<GeoSeedOptions>(builder.Configuration.GetSection("GeoSeed"));
builder.Services.Configure<WsuOptions>(builder.Configuration.GetSection("Wsu"));
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            RoleClaimType = ERP.Modules.Identity.Application.Security.IdentityClaimTypes.Role,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),
            ClockSkew = TimeSpan.FromMinutes(1)
        };
});
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(TenantGuardAttribute.PolicyName, policy =>
        policy.RequireAuthenticatedUser().AddRequirements(new TenantGuardRequirement()));
});

builder.Services.Configure<ApiJwtOptions>(options =>
{
    builder.Configuration.GetSection("Jwt").Bind(options);
    if (string.IsNullOrWhiteSpace(options.SigningKey))
    {
        var signingKeyFromEnvironment = builder.Configuration["JWT_SIGNING_KEY"];
        options.SigningKey = isProduction
            ? signingKeyFromEnvironment ?? string.Empty
            : signingKeyFromEnvironment ?? devJwtSigningKey;
    }

    if (isProduction && string.IsNullOrWhiteSpace(options.SigningKey))
    {
        throw new InvalidOperationException("JWT signing key is not configured. Set Jwt:SigningKey or JWT_SIGNING_KEY.");
    }
});

builder.Services.Configure<IdentityJwtOptions>(options =>
{
    builder.Configuration.GetSection("Jwt").Bind(options);
    if (string.IsNullOrWhiteSpace(options.SigningKey))
    {
        var signingKeyFromEnvironment = builder.Configuration["JWT_SIGNING_KEY"];
        options.SigningKey = isProduction
            ? signingKeyFromEnvironment ?? string.Empty
            : signingKeyFromEnvironment ?? devJwtSigningKey;
    }

    if (isProduction && string.IsNullOrWhiteSpace(options.SigningKey))
    {
        throw new InvalidOperationException("JWT signing key is not configured. Set Jwt:SigningKey or JWT_SIGNING_KEY.");
    }
});

builder.Services.AddAntiforgery(options =>
{
    options.Cookie.Name = authHttpOptions.ResolveAntiforgeryCookieName();
    options.Cookie.HttpOnly = true;
    options.Cookie.Path = "/";
    options.Cookie.SecurePolicy = authHttpOptions.ResolveCookieSecurePolicy();
    options.Cookie.SameSite = authHttpOptions.ResolveSameSiteMode();
    options.HeaderName = "X-XSRF-TOKEN";
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("ApiCors", policy =>
    {
        policy.WithOrigins(authHttpOptions.AllowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });

    options.AddPolicy("SpaAuthCors", policy =>
    {
        policy.WithOrigins(authHttpOptions.AllowedOrigins)
            .WithHeaders("Authorization", "Content-Type", "X-XSRF-TOKEN")
            .WithMethods("POST", "OPTIONS")
            .AllowCredentials();
    });
});

builder.Services.AddScoped<IEventPublisher, EventPublisher>();
builder.Services.AddScoped<ITaxCalculationQuery, TaxCalculationQuery>();
builder.Services.AddScoped<IDocumentSearch, PostgresDocumentSearch>();
builder.Services.AddScoped<ICompanyFeatureRepository, CompanyFeatureRepository>();
builder.Services.AddMemoryCache();
builder.Services.AddScoped<ICompanyContext, HttpContextCompanyContext>();
builder.Services.AddScoped<IFeatureCatalogService, FeatureCatalogService>();
builder.Services.AddScoped<ICompanyFeatureService, CompanyFeatureService>();
builder.Services.AddScoped<IFeatureService, CompanyFeatureService>();
builder.Services.AddScoped<ICompanyFeatureManager, CompanyFeatureManager>();
builder.Services.AddScoped<IPlanRepository, PlanRepository>();
builder.Services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
builder.Services.AddScoped<SubscriptionService>();
builder.Services.AddScoped<IWebpayGateway, WebpayGateway>();
builder.Services.AddSingleton<IAutomationProvider, NoOpAutomationProvider>();
builder.Services.AddSingleton<IAutomationProviderRegistry, InMemoryAutomationProviderRegistry>();
builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
builder.Services.AddScoped<IAuthorizationHandler, TenantGuardHandler>();
builder.Services.AddScoped<IAuthorizationHandler, ScopedPermissionAuthorizationHandler>();
builder.Services.AddScoped<IAuthorizationHandler, CompanyMembershipAuthorizationHandler>();
builder.Services.AddSingleton<IAuthorizationPolicyProvider, ScopedPermissionPolicyProvider>();
builder.Services.AddScoped<ICompanyLookup, CompanyLookup>();
builder.Services.AddScoped<ERP.Modules.MasterData.Contracts.ICompanyRepository, CompanyRepository>();
builder.Services.AddScoped<ERP.Modules.MasterData.Application.Repositories.ICompanyRepository, CompanyRepository>();
builder.Services.AddScoped<ERP.Modules.MasterData.Contracts.IOrganizationRepository, OrganizationRepository>();
builder.Services.AddScoped<ERP.Modules.Identity.Application.Repositories.ICompanyLookupRepository, CompanyRepository>();
builder.Services.AddScoped<ERP.Modules.Identity.Application.Repositories.ITaxEntityRepository, TaxEntityRepository>();
builder.Services.AddScoped<IPharmaProductProfileRepository, PharmaProductProfileRepository>();
builder.Services.AddScoped<IProductPharmaInfoRepository, ProductPharmaInfoRepository>();
builder.Services.AddScoped<IStockBatchRepository, StockBatchRepository>();
builder.Services.AddScoped<IStockLedgerEntryRepository, StockLedgerEntryRepository>();
builder.Services.AddScoped<IWarehouseLocationRepository, WarehouseLocationRepository>();
builder.Services.AddScoped<IWarehouseOperationRepository, WarehouseOperationRepository>();
builder.Services.AddScoped<IDispatchConfirmationRepository, DispatchConfirmationRepository>();
builder.Services.AddScoped<IQuarantineHoldRepository, QuarantineHoldRepository>();
builder.Services.AddScoped<IBlockHoldRepository, BlockHoldRepository>();
builder.Services.AddScoped<IRecallRepository, RecallRepository>();
builder.Services.AddScoped<IWasteRepository, WasteRepository>();
builder.Services.AddScoped<IPharmacyCriticalAuditRepository, PharmacyCriticalAuditRepository>();
builder.Services.AddScoped<IMovementBatchAllocationRepository, MovementBatchAllocationRepository>();
builder.Services.AddScoped<IPharmacyTraceEventRepository, PharmacyTraceEventRepository>();
builder.Services.AddScoped<IDispenseRepository, DispenseRepository>();
builder.Services.AddScoped<IOfficialControlledBookEntryRepository, OfficialControlledBookEntryRepository>();
builder.Services.AddScoped<IControlledSubstanceLedgerRepository, ControlledSubstanceLedgerRepository>();
builder.Services.AddScoped<IControlledSubstanceLedgerAuditRepository, ControlledSubstanceLedgerAuditRepository>();
builder.Services.AddScoped<IControlledSubstanceReportExportRepository, ControlledSubstanceReportExportRepository>();
builder.Services.AddScoped<IExpirationAlertRuleRepository, ExpirationAlertRuleRepository>();
builder.Services.AddScoped<IStockoutThresholdRepository, StockoutThresholdRepository>();
builder.Services.AddScoped<IPurchaseInvoiceIngestionRepository, PurchaseInvoiceIngestionRepository>();
builder.Services.AddScoped<IDeliveryConfirmationRepository, DeliveryConfirmationRepository>();
builder.Services.AddScoped<IPharmacyReportsQuery, PharmacyReportsQuery>();
builder.Services.AddScoped<IPharmacySalesAssistQuery, PharmacySalesAssistQuery>();
builder.Services.AddScoped<ControlledSubstanceLedgerCommandHandler>();
builder.Services.AddScoped<ERP.Modules.MasterData.Application.Handlers.SupplierCommandHandler>();
builder.Services.AddScoped<ERP.Modules.MasterData.Application.Handlers.SupplierQueryHandler>();
builder.Services.AddScoped<ERP.Modules.MasterData.Application.Handlers.ProductSupplierCommandHandler>();
builder.Services.AddScoped<ERP.Modules.MasterData.Application.Handlers.ProductSupplierQueryHandler>();
builder.Services.AddScoped<IPurchaseInvoiceQrParser, JsonPurchaseInvoiceQrParser>();
builder.Services.AddScoped<PharmacyDispensingService>();
builder.Services.AddScoped<PharmacyInventoryService>();
builder.Services.AddScoped<ControlledSubstanceReportService>();
builder.Services.AddScoped<WarehouseLayoutService>();
builder.Services.AddScoped<IDocumentRepository, InMemoryDocumentRepository>();
builder.Services.AddScoped<DocumentCommandHandler>();
builder.Services.AddScoped<DocumentQueryHandler>();
builder.Services.AddScoped<IEventHandler<ERP.Modules.Inventory.Domain.StockMoved>, StockMovedHandler>();
builder.Services.AddScoped<IEventHandler<ERP.Modules.PharmaceuticalRegulatedInventory.Domain.DispenseConfirmed>, DispenseConfirmedHandler>();
builder.Services.AddScoped<IEventHandler<ERP.Modules.PharmaceuticalRegulatedInventory.Domain.ControlledSubstanceLedgerEntryRecorded>, ControlledSubstanceLedgerAuditHandler>();
builder.Services.AddScoped<IEventHandler<ERP.Modules.Accounting.Contracts.AccountingPostRequested>, ERP.Modules.Accounting.Application.Handlers.AccountingPostRequestedHandler>();
builder.Services.AddScoped<IUnitOfWork, ErpUnitOfWork>();
builder.Services.AddScoped<TaxEntityService>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ERP.Modules.MasterData.Contracts.IProductBarcodeLookup, ProductRepository>();
builder.Services.AddScoped<ISupplierRepository, SupplierRepository>();
builder.Services.AddScoped<IProductSupplierRepository, ProductSupplierRepository>();
        builder.Services.AddScoped<IUnitOfMeasureRepository, UnitOfMeasureRepository>();
        builder.Services.AddScoped<IUnitOfMeasureTranslationRepository, UnitOfMeasureTranslationRepository>();
        builder.Services.AddScoped<IUnitOfMeasureExternalMappingRepository, UnitOfMeasureExternalMappingRepository>();
        builder.Services.AddScoped<ICompanyUnitOfMeasureRepository, CompanyUnitOfMeasureRepository>();
        builder.Services.AddScoped<ICompanyCurrencyRepository, CompanyCurrencyRepository>();
        builder.Services.AddScoped<ICurrencyRepository, CurrencyRepository>();
        builder.Services.AddScoped<ICurrencySeedService, CurrencySeedService>();
builder.Services.AddScoped<IProductUnitConversionRepository, ProductUnitConversionRepository>();
builder.Services.AddScoped<IWarehouseRepository, WarehouseRepository>();
builder.Services.AddScoped<ICountryRepository, CountryRepository>();
builder.Services.AddScoped<ISubdivisionRepository, SubdivisionRepository>();
builder.Services.AddScoped<ICityRepository, CityRepository>();
builder.Services.AddScoped<ILocalityRepository, LocalityRepository>();
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<IAccountingAccountTemplateRepository, AccountingAccountTemplateRepository>();
builder.Services.AddScoped<IJournalEntryRepository, JournalEntryRepository>();
builder.Services.AddScoped<IJournalRepository, JournalRepository>();
builder.Services.AddScoped<IAccountingPeriodRepository, AccountingPeriodRepository>();
builder.Services.AddScoped<ICompanyAccountingSettingsRepository, CompanyAccountingSettingsRepository>();
builder.Services.AddScoped<IDteDocumentRepository, DteDocumentRepository>();
builder.Services.AddScoped<ITaxBookEntryRepository, TaxBookEntryRepository>();
builder.Services.AddScoped<ITaxDeclarationRepository, TaxDeclarationRepository>();
builder.Services.AddScoped<IAccountingAutomationRuleRepository, AccountingAutomationRuleRepository>();
builder.Services.AddScoped<IInventoryMovementRepository, InventoryMovementRepository>();
builder.Services.AddScoped<IStockRepository, StockRepository>();
builder.Services.AddScoped<IPurchaseOrderRepository, PurchaseOrderRepository>();
builder.Services.AddScoped<IGoodsReceiptRepository, GoodsReceiptRepository>();
builder.Services.AddScoped<IPurchaseSuggestionRepository, PurchaseSuggestionRepository>();
builder.Services.AddScoped<IPurchaseSuggestionDataQuery, PurchaseSuggestionDataQuery>();
builder.Services.AddScoped<IPurchaseOrderQuery, PurchaseOrderQueryRepository>();
builder.Services.AddScoped<IGoodsReceiptQuery, GoodsReceiptQueryRepository>();
builder.Services.AddScoped<ISalesDocumentRepository, SalesDocumentRepository>();
builder.Services.AddScoped<ISalesDocumentQuery, SalesDocumentQueryRepository>();
builder.Services.AddScoped<ISalesQuoteRepository, SalesQuoteRepository>();
builder.Services.AddScoped<IPriceListRepository, PriceListRepository>();
builder.Services.AddScoped<IPriceListItemRepository, PriceListItemRepository>();
builder.Services.AddScoped<IPricingPolicyRepository, PricingPolicyRepository>();
builder.Services.AddScoped<AccountingAccountsReceivableRepository, AccountsReceivableRepository>();
builder.Services.AddScoped<IUserProfileRepository, UserProfileRepository>();
builder.Services.AddScoped<IOutboxRepository, OutboxRepository>();
builder.Services.AddScoped<ERP.Workflows.Application.Repositories.IWorkflowDefinitionRepository, WorkflowDefinitionRepository>();
builder.Services.AddScoped<ERP.Workflows.Application.Repositories.IWorkflowInstanceRepository, WorkflowInstanceRepository>();
builder.Services.AddScoped<ERP.Workflows.Application.Repositories.IWorkflowTaskRepository, WorkflowTaskRepository>();
builder.Services.AddScoped<ERP.Workflows.Application.Repositories.IWorkflowHistoryRepository, WorkflowHistoryRepository>();
builder.Services.AddScoped<IAssetCategoryRepository, InMemoryAssetCategoryRepository>();
builder.Services.AddScoped<IFixedAssetRepository, InMemoryFixedAssetRepository>();
builder.Services.AddScoped<IDepreciationScheduleRepository, InMemoryDepreciationScheduleRepository>();
builder.Services.AddScoped<IAssetCategoryRepository, AssetCategoryRepository>();
builder.Services.AddScoped<IFixedAssetRepository, FixedAssetRepository>();
builder.Services.AddScoped<IDepreciationScheduleRepository, DepreciationScheduleRepository>();
builder.Services.AddScoped<ITaxRepository, TaxRepository>();
builder.Services.AddScoped<ITaxGroupRepository, TaxGroupRepository>();
builder.Services.AddScoped<IBankAccountRepository, BankAccountRepository>();
builder.Services.AddScoped<IBankStatementRepository, BankStatementRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IReceiptRepository, ReceiptRepository>();
builder.Services.AddScoped<AccountingAccountsPayableRepository, AccountsPayableRepository>();
builder.Services.AddScoped<IInvoiceRepository, InvoiceRepository>();
builder.Services.AddScoped<ICreditNoteRepository, CreditNoteRepository>();
builder.Services.AddScoped<IRfidTagRepository, RfidTagRepository>();
builder.Services.AddScoped<IRfidEventInboxRepository, RfidEventInboxRepository>();
builder.Services.AddScoped<IRfidAccessSessionRepository, RfidAccessSessionRepository>();
builder.Services.AddScoped<IRfidMovementLinkRepository, RfidMovementLinkRepository>();
builder.Services.AddScoped<IRfidOperatorRepository, RfidOperatorRepository>();
builder.Services.AddScoped<IRfidOperatorCredentialRepository, RfidOperatorCredentialRepository>();
builder.Services.AddScoped<IRfidReferenceResolver, RfidReferenceResolver>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderItemRepository, OrderItemRepository>();
builder.Services.AddScoped<IOrderItemConsumptionRepository, OrderItemConsumptionRepository>();
builder.Services.AddScoped<IOrderItemReconciliationRepository, OrderItemReconciliationRepository>();
builder.Services.AddScoped<IOrderItemEpcAssignmentRepository, OrderItemEpcAssignmentRepository>();
builder.Services.AddScoped<IWsuOrderMovementOperatorRepository, WsuOrderMovementOperatorRepository>();
builder.Services.AddScoped<IWsuRfidTagLookupRepository, WsuRfidTagLookupRepository>();
builder.Services.AddScoped<IWsuInventoryMovementOperatorRepository, WsuInventoryMovementOperatorRepository>();
builder.Services.AddScoped<IOperatorEnrollmentValidator, OperatorEnrollmentValidator>();
builder.Services.AddScoped<IWsuInventoryMovementRepository, WsuInventoryMovementRepository>();
builder.Services.AddScoped<IDebitNoteRepository, DebitNoteRepository>();
builder.Services.AddScoped<IUnitOfMeasureCatalog, UnitOfMeasureCatalogService>();
builder.Services.AddScoped<ICompanyUnitOfMeasureService, CompanyUnitOfMeasureService>();
builder.Services.AddScoped<IUnitOfMeasureAccessValidator, UnitOfMeasureAccessValidator>();
builder.Services.AddScoped<IUnitOfMeasureSeedService, UnitOfMeasureSeedService>();
builder.Services.AddScoped<IUnitOfMeasureConversionService, UnitOfMeasureConversionService>();
builder.Services.AddScoped<IGeoCatalogService, GeoCatalogService>();
builder.Services.AddScoped<IGeoSeedSource, PersistenceGeoSeedSource>();
builder.Services.AddScoped<IGeoSeedService, GeoSeedService>();
builder.Services.AddScoped<IAddressValidator, AddressValidationService>();
builder.Services.AddScoped<ICompanyCurrencyValidationService, CompanyCurrencyValidationService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserSessionRepository, UserSessionRepository>();
        builder.Services.AddScoped<IRoleRepository, RoleRepository>();
        builder.Services.AddScoped<IRoleAssignmentRepository, RoleAssignmentRepository>();
builder.Services.AddScoped<ICompanyAccessRepository, CompanyAccessRepository>();
builder.Services.AddScoped<ICompanyAccessResolver, CompanyAccessResolver>();
builder.Services.AddScoped<IWorkspaceResolver, WorkspaceResolver>();
        builder.Services.AddScoped<IWorkspaceOrganizationResolver, WorkspaceOrganizationResolver>();
builder.Services.AddScoped<ICompanyAccessSource, DirectCompanyUsersSource>();
builder.Services.AddScoped<ICompanyAccessSource, HoldingOrganizationCompaniesSource>();
builder.Services.AddScoped<ICompanyAccessSource, MultiCompanyPortfolioSource>();
        builder.Services.AddScoped<ICompanyUserRepository, CompanyUserRepository>();
builder.Services.AddScoped<IOrganizationMemberRepository, OrganizationMemberRepository>();
builder.Services.AddScoped<ICompanyLinkRepository, CompanyLinkRepository>();
builder.Services.AddScoped<IPermissionRepository, PermissionRepository>();
builder.Services.AddScoped<IRolePermissionRepository, RolePermissionRepository>();
builder.Services.AddScoped<IRbacService, RbacService>();
builder.Services.AddScoped<RbacSeedService>();
builder.Services.AddScoped<FeatureCatalogSeedService>();
builder.Services.AddScoped<FinanceAccountsReceivableRepository, AccountsReceivableRepository>();
builder.Services.AddScoped<FinanceAccountsPayableRepository, AccountsPayableRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IReceiptRepository, ReceiptRepository>();
builder.Services.AddScoped<IBankStatementRepository, BankStatementRepository>();
builder.Services.AddScoped<IBankTransactionRepository, BankTransactionRepository>();
builder.Services.AddScoped<IInvoiceRepository, InvoiceRepository>();
builder.Services.AddScoped<ICreditNoteRepository, CreditNoteRepository>();


builder.Services.AddScoped<MasterDataService>();
builder.Services.AddScoped<CompanyCurrencyService>();
builder.Services.AddScoped<IInventoryFlowService, InventoryFlowService>();
builder.Services.AddScoped<PurchasingService>();
builder.Services.AddScoped<SalesService>();
builder.Services.AddScoped<CreateSalesQuoteHandler>();
builder.Services.AddScoped<SubmitSalesQuoteHandler>();
builder.Services.AddScoped<ApproveSalesQuoteHandler>();
builder.Services.AddScoped<RejectSalesQuoteHandler>();
builder.Services.AddScoped<SalesQuoteQueryHandler>();
builder.Services.AddScoped<PricingService>();
builder.Services.AddScoped<PricingPolicyService>();
builder.Services.AddScoped<ReportsService>();
builder.Services.AddScoped<WsuOrderService>(serviceProvider =>
{
    var options = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<WsuOptions>>().Value;

    return new WsuOrderService(
        serviceProvider.GetRequiredService<IOrderRepository>(),
        serviceProvider.GetRequiredService<IOrderItemRepository>(),
        serviceProvider.GetRequiredService<IOrderItemConsumptionRepository>(),
        serviceProvider.GetRequiredService<IOrderItemReconciliationRepository>(),
        serviceProvider.GetRequiredService<IOrderItemEpcAssignmentRepository>(),
        serviceProvider.GetRequiredService<IWsuOrderMovementOperatorRepository>(),
        serviceProvider.GetRequiredService<IWsuRfidTagLookupRepository>(),
        serviceProvider.GetRequiredService<IOperatorEnrollmentValidator>(),
        serviceProvider.GetRequiredService<ERP.Modules.MasterData.Application.Repositories.IProductRepository>(),
        serviceProvider.GetRequiredService<ERP.Modules.MasterData.Application.Repositories.ICompanyUnitOfMeasureRepository>(),
        serviceProvider.GetRequiredService<ERP.Modules.MasterData.Application.Repositories.ICompanyRepository>(),
        serviceProvider.GetRequiredService<IWsuInventoryMovementRepository>(),
        serviceProvider.GetRequiredService<IUnitOfWork>(),
        options.ValidarOperadorEnOrden);
});
builder.Services.AddScoped<FixedAssetService>();
builder.Services.AddScoped<TaxService>();
builder.Services.AddScoped<CashApplicationService>();
builder.Services.AddScoped<BankReconciliationService>();
builder.Services.AddScoped<PayablesService>();
builder.Services.AddScoped<ERP.Workflows.Application.Services.WorkflowRuleEvaluator>();
builder.Services.AddScoped<ERP.Workflows.Application.Services.WorkflowService>();
builder.Services.AddScoped<ReceivablesService>();
builder.Services.AddScoped<BillingNumberingService>();
builder.Services.AddScoped<BillingIssuanceService>();
builder.Services.AddScoped<BillingStatusService>();
builder.Services.AddScoped<IdentityService>();
builder.Services.AddScoped<CompanyAccessService>();
builder.Services.AddScoped<BootstrapCompanyService>();
builder.Services.AddScoped<TaxCalculator>();
builder.Services.AddScoped<FinanceService>();
builder.Services.AddScoped<CashService>();
builder.Services.AddScoped<BillingService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<RoleService>();
builder.Services.AddScoped<PermissionService>();
builder.Services.AddScoped<UserPermissionsService>();
builder.Services.AddSingleton<IIdentityEndpointRateLimiter, InMemoryIdentityEndpointRateLimiter>();
builder.Services.AddScoped<IAccessService, AccessService>();
builder.Services.AddScoped<OnboardingService>();
builder.Services.AddScoped<ITokenService, JwtTokenService>();
builder.Services.AddScoped<IPasswordHasher, Pbkdf2PasswordHasher>();

// Password Reset Services
builder.Services.Configure<PasswordResetOptions>(builder.Configuration.GetSection("PasswordReset"));
builder.Services.Configure<PasswordReusePolicyOptions>(builder.Configuration.GetSection("PasswordReusePolicy"));
builder.Services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();
builder.Services.AddScoped<IUserPasswordHistoryRepository, UserPasswordHistoryRepository>();
builder.Services.AddScoped<IPasswordResetTokenService, PasswordResetTokenService>();
builder.Services.AddScoped<IPasswordReuseValidator, PasswordReuseValidator>();
builder.Services.AddScoped<ISessionInvalidationService, SessionInvalidationService>();
builder.Services.AddScoped<PasswordResetService>();
builder.Services.AddScoped<RfidService>();
builder.Services.AddScoped<AccountingService>();
builder.Services.AddScoped<AccountingPostingService>();
builder.Services.AddScoped<AccountingAutomationEngine>();
builder.Services.AddScoped<AccountingChartSeedService>();
builder.Services.AddScoped<AccountingBootstrapService>();
builder.Services.AddScoped<AccountingPeriodService>();
builder.Services.AddScoped<AccountingAuditService>();
builder.Services.AddScoped<TaxComplianceService>();
builder.Services.AddScoped<UserProfileService>();
builder.Services.AddScoped<InitialSeedService>();
builder.Services.AddScoped<DevSeedService>();
builder.Services.AddScoped<RfidSeedService>();
builder.Services.AddScoped<ITenantContext, HttpContextTenantContext>();
builder.Services.AddScoped<IChartOfAccountsTemplateSource, JsonChartOfAccountsTemplateSource>();

// Add email notifications with hot reload support
builder.Services.AddNotifications(builder.Configuration);

// Add email smoke test service
builder.Services.AddScoped<ERP.Modules.Identity.Application.Services.EmailSmokeTestService>();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

    options.KnownProxies.Clear();     // válido
    options.KnownIPNetworks.Clear();  // reemplaza a KnownNetworks
});

var app = builder.Build();

app.UseForwardedHeaders();
app.UseRouting();

app.UseCors("ApiCors");

app.UseSwagger(options =>
{
    options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_1;
});
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "ERP API v1");
    options.DisplayRequestDuration();

    // Force single-column (full width) layout so tag descriptions look good
    options.ConfigObject.AdditionalItems["layout"] = "BaseLayout";
});

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.MapGet("/api/health", () => Results.Ok("OK"));

app.MapPost("/api/seed/global", async (
    InitialSeedService initialSeedService,
    HttpRequest request,
    IConfiguration configuration,
    ILogger<Program> logger,
    CancellationToken cancellationToken = default) =>
{
    var seedKey = configuration["GlobalSeed:Key"];
    if (string.IsNullOrWhiteSpace(seedKey))
    {
        return Results.Problem("GlobalSeed:Key is not configured.", statusCode: StatusCodes.Status500InternalServerError);
    }

    if (!request.Headers.TryGetValue("X-GLOBAL-SEED-KEY", out var providedKey)
        || !string.Equals(providedKey.ToString(), seedKey, StringComparison.Ordinal))
    {
        return Results.Unauthorized();
    }

    try
    {
        var result = await initialSeedService.SeedGlobalCatalogAsync(cancellationToken);
        return Results.Ok(result);
    }
    catch (InvalidOperationException ex)
    {
        return Results.Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Global seed failed.");
        return Results.Problem("Failed to run the global seed.", statusCode: StatusCodes.Status500InternalServerError);
    }
});

app.MapPost("/api/seed/features", async (
    InitialSeedService initialSeedService,
    HttpRequest request,
    IConfiguration configuration,
    ILogger<Program> logger,
    CancellationToken cancellationToken = default) =>
{
    var seedKey = configuration["GlobalSeed:Key"];
    if (string.IsNullOrWhiteSpace(seedKey))
    {
        return Results.Problem("GlobalSeed:Key is not configured.", statusCode: StatusCodes.Status500InternalServerError);
    }

    if (!request.Headers.TryGetValue("X-GLOBAL-SEED-KEY", out var providedKey)
        || !string.Equals(providedKey.ToString(), seedKey, StringComparison.Ordinal))
    {
        return Results.Unauthorized();
    }

    try
    {
        var result = await initialSeedService.SeedFeaturesAsync(cancellationToken);
        return Results.Ok(result);
    }
    catch (InvalidOperationException ex)
    {
        return Results.Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Features seed failed.");
        return Results.Problem("Failed to run the features seed.", statusCode: StatusCodes.Status500InternalServerError);
    }
});

app.MapPost("/api/seed/global/rbac", async (
    InitialSeedService initialSeedService,
    HttpRequest request,
    IConfiguration configuration,
    ILogger<Program> logger,
    CancellationToken cancellationToken = default) =>
{
    var seedKey = configuration["GlobalSeed:Key"];
    if (string.IsNullOrWhiteSpace(seedKey))
    {
        return Results.Problem("GlobalSeed:Key is not configured.", statusCode: StatusCodes.Status500InternalServerError);
    }

    if (!request.Headers.TryGetValue("X-GLOBAL-SEED-KEY", out var providedKey)
        || !string.Equals(providedKey.ToString(), seedKey, StringComparison.Ordinal))
    {
        return Results.Unauthorized();
    }

    try
    {
        var result = await initialSeedService.SeedRbacAsync(cancellationToken);
        return Results.Ok(result);
    }
    catch (InvalidOperationException ex)
    {
        return Results.Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Global RBAC seed failed.");
        return Results.Problem("Failed to run the global RBAC seed.", statusCode: StatusCodes.Status500InternalServerError);
    }
});

app.MapPost("/api/seed/global/rbac/otros", async (
    InitialSeedService initialSeedService,
    HttpRequest request,
    IConfiguration configuration,
    ILogger<Program> logger,
    CancellationToken cancellationToken = default) =>
{
    var seedKey = configuration["GlobalSeed:Key"];
    if (string.IsNullOrWhiteSpace(seedKey))
    {
        return Results.Problem("GlobalSeed:Key is not configured.", statusCode: StatusCodes.Status500InternalServerError);
    }

    if (!request.Headers.TryGetValue("X-GLOBAL-SEED-KEY", out var providedKey)
        || !string.Equals(providedKey.ToString(), seedKey, StringComparison.Ordinal))
    {
        return Results.Unauthorized();
    }

    try
    {
        var result = await initialSeedService.SeedRbacOthersAsync(cancellationToken);
        return Results.Ok(result);
    }
    catch (InvalidOperationException ex)
    {
        return Results.Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Global supplementary RBAC seed failed.");
        return Results.Problem("Failed to run the global supplementary RBAC seed.", statusCode: StatusCodes.Status500InternalServerError);
    }
});

app.MapPost("/api/seed/global/accounting", async (
    InitialSeedService initialSeedService,
    HttpRequest request,
    IConfiguration configuration,
    ILogger<Program> logger,
    CancellationToken cancellationToken = default) =>
{
    var seedKey = configuration["GlobalSeed:Key"];
    if (string.IsNullOrWhiteSpace(seedKey))
    {
        return Results.Problem("GlobalSeed:Key is not configured.", statusCode: StatusCodes.Status500InternalServerError);
    }

    if (!request.Headers.TryGetValue("X-GLOBAL-SEED-KEY", out var providedKey)
        || !string.Equals(providedKey.ToString(), seedKey, StringComparison.Ordinal))
    {
        return Results.Unauthorized();
    }

    try
    {
        var result = await initialSeedService.SeedGlobalAccountingAsync(cancellationToken);
        return Results.Ok(result);
    }
    catch (InvalidOperationException ex)
    {
        return Results.Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Global accounting seed failed.");
        return Results.Problem("Failed to run the global accounting seed.", statusCode: StatusCodes.Status500InternalServerError);
    }
});

app.MapPost("/api/seed/rfid", async (
    RfidSeedService rfidSeedService,
    HttpRequest request,
    IConfiguration configuration,
    ILogger<Program> logger,
    CancellationToken cancellationToken = default) =>
{
    var seedKey = configuration["GlobalSeed:Key"];
    if (string.IsNullOrWhiteSpace(seedKey))
    {
        return Results.Problem("GlobalSeed:Key is not configured.", statusCode: StatusCodes.Status500InternalServerError);
    }

    if (!request.Headers.TryGetValue("X-GLOBAL-SEED-KEY", out var providedKey)
        || !string.Equals(providedKey.ToString(), seedKey, StringComparison.Ordinal))
    {
        return Results.Unauthorized();
    }

    try
    {
        var result = await rfidSeedService.SeedAsync(cancellationToken);
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "RFID seed failed.");
        return Results.Problem("Failed to run the RFID seed.", statusCode: StatusCodes.Status500InternalServerError);
    }
});

app.MapPost("/api/seed/rfid/uom", async (
    RfidSeedService rfidSeedService,
    HttpRequest request,
    IConfiguration configuration,
    ILogger<Program> logger,
    CancellationToken cancellationToken = default) =>
{
    var seedKey = configuration["GlobalSeed:Key"];
    if (string.IsNullOrWhiteSpace(seedKey))
    {
        return Results.Problem("GlobalSeed:Key is not configured.", statusCode: StatusCodes.Status500InternalServerError);
    }

    if (!request.Headers.TryGetValue("X-GLOBAL-SEED-KEY", out var providedKey)
        || !string.Equals(providedKey.ToString(), seedKey, StringComparison.Ordinal))
    {
        return Results.Unauthorized();
    }

    try
    {
        var result = await rfidSeedService.SeedMasterCompanyUomAsync(cancellationToken);
        return Results.Ok(result);
    }
    catch (InvalidOperationException ex)
    {
        return Results.Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "RFID UOM seed failed.");
        return Results.Problem("Failed to run the RFID UOM seed.", statusCode: StatusCodes.Status500InternalServerError);
    }
});

if (app.Environment.IsDevelopment())
{
    static string? GetDevSeedConfigurationError(HttpRequest request, IConfiguration configuration)
    {
        var seedKey = configuration["DevSeed:Key"];
        if (string.IsNullOrWhiteSpace(seedKey))
        {
            return "DevSeed:Key is not configured.";
        }

        if (!request.Headers.TryGetValue("X-DEV-SEED-KEY", out var providedKey)
            || !string.Equals(providedKey.ToString(), seedKey, StringComparison.Ordinal))
        {
            return "unauthorized";
        }

        return null;
    }

    app.MapPost("/api/dev/seed/base", async (
        DevSeedService devSeedService,
        ILogger<Program> logger,
        HttpRequest request,
        IConfiguration configuration,
        bool force = false,
        CancellationToken cancellationToken = default) =>
    {
        try
        {
            var validationError = GetDevSeedConfigurationError(request, configuration);
            if (validationError is not null)
            {
                return validationError == "unauthorized"
                    ? Results.Unauthorized()
                    : Results.Problem(validationError, statusCode: StatusCodes.Status500InternalServerError);
            }

            var result = await devSeedService.SeedBaseAsync(force, cancellationToken);
            return Results.Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return Results.Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Dev seed base failed.");
            return Results.Problem("Failed to run the dev seed base.", statusCode: StatusCodes.Status500InternalServerError);
        }
    });

    app.MapPost("/api/dev/seed/accounting", async (
        DevSeedService devSeedService,
        ILogger<Program> logger,
        HttpRequest request,
        IConfiguration configuration,
        bool force = false,
        CancellationToken cancellationToken = default) =>
    {
        try
        {
            var validationError = GetDevSeedConfigurationError(request, configuration);
            if (validationError is not null)
            {
                return validationError == "unauthorized"
                    ? Results.Unauthorized()
                    : Results.Problem(validationError, statusCode: StatusCodes.Status500InternalServerError);
            }

            var result = await devSeedService.SeedAccountingAsync(force, cancellationToken);
            return Results.Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return Results.Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Dev seed accounting failed.");
            return Results.Problem("Failed to run the dev seed accounting.", statusCode: StatusCodes.Status500InternalServerError);
        }
    });

    app.MapPost("/api/dev/seed/prices", async (
        DevSeedService devSeedService,
        ILogger<Program> logger,
        HttpRequest request,
        IConfiguration configuration,
        bool force = false,
        CancellationToken cancellationToken = default) =>
    {
        try
        {
            var validationError = GetDevSeedConfigurationError(request, configuration);
            if (validationError is not null)
            {
                return validationError == "unauthorized"
                    ? Results.Unauthorized()
                    : Results.Problem(validationError, statusCode: StatusCodes.Status500InternalServerError);
            }

            var result = await devSeedService.SeedPricesAsync(force, cancellationToken);
            return Results.Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return Results.Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Dev seed prices failed.");
            return Results.Problem("Failed to run the dev seed prices.", statusCode: StatusCodes.Status500InternalServerError);
        }
    });

    app.MapPost("/api/dev/seed/operation", async (
        DevSeedService devSeedService,
        ILogger<Program> logger,
        HttpRequest request,
        IConfiguration configuration,
        bool force = false,
        CancellationToken cancellationToken = default) =>
    {
        try
        {
            var validationError = GetDevSeedConfigurationError(request, configuration);
            if (validationError is not null)
            {
                return validationError == "unauthorized"
                    ? Results.Unauthorized()
                    : Results.Problem(validationError, statusCode: StatusCodes.Status500InternalServerError);
            }

            var result = await devSeedService.SeedOperationAsync(force, cancellationToken);
            return Results.Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return Results.Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Dev seed operation failed.");
            return Results.Problem("Failed to run the dev seed operation.", statusCode: StatusCodes.Status500InternalServerError);
        }
    });
}

await app.RunAsync();

public partial class Program { }
