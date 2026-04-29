using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using NpgsqlTypes;

#nullable disable

namespace ERP.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "rfid");

            migrationBuilder.EnsureSchema(
                name: "masterdata");

            migrationBuilder.EnsureSchema(
                name: "pharma");

            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.EnsureSchema(
                name: "iam");

            migrationBuilder.EnsureSchema(
                name: "wms");

            migrationBuilder.EnsureSchema(
                name: "documents");

            migrationBuilder.EnsureSchema(
                name: "sales");

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: true),
                    Action = table.Column<string>(type: "text", nullable: false),
                    EntityName = table.Column<string>(type: "text", nullable: false),
                    EntityId = table.Column<string>(type: "text", nullable: false),
                    ChangesJson = table.Column<string>(type: "text", nullable: false),
                    Source = table.Column<string>(type: "text", nullable: false),
                    OccurredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChartOfAccountsTemplates",
                schema: "masterdata",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    AccountType = table.Column<string>(type: "text", nullable: false),
                    ParentId = table.Column<long>(type: "bigint", nullable: true),
                    IsSystemRequired = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Version = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SystemRole = table.Column<string>(type: "text", nullable: true),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChartOfAccountsTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Country",
                schema: "masterdata",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Iso2 = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    Iso3 = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    Name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    NumericCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    PhonePrefix = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: true),
                    CurrencyCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Country", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Currency",
                schema: "masterdata",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    NumericCode = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Symbol = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    MinorUnits = table.Column<int>(type: "integer", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Currency", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Documents",
                schema: "documents",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyPublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Documents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DocumentSearchEntries",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DocumentType = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DocumentId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    Content = table.Column<string>(type: "text", nullable: false),
                    Metadata = table.Column<Dictionary<string, string>>(type: "jsonb", nullable: false, defaultValueSql: "'{}'::jsonb"),
                    SearchVector = table.Column<NpgsqlTsVector>(type: "tsvector", nullable: true, computedColumnSql: "to_tsvector('simple', coalesce(\"Title\", '') || ' ' || coalesce(\"Content\", ''))", stored: true),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompanyId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentSearchEntries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EventInbox",
                schema: "rfid",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyPublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    EventType = table.Column<int>(type: "integer", nullable: false),
                    DeviceCode = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    WarehousePublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Confidence = table.Column<decimal>(type: "numeric(5,4)", precision: 5, scale: 4, nullable: true),
                    PayloadJson = table.Column<string>(type: "text", nullable: false),
                    ProcessingStatus = table.Column<int>(type: "integer", nullable: false),
                    ProcessedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Error = table.Column<string>(type: "text", nullable: true),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventInbox", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FeatureCatalog",
                schema: "masterdata",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeatureCatalog", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MovementLinks",
                schema: "rfid",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyPublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SessionId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Epc = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ProductPublicId = table.Column<Guid>(type: "uuid", nullable: true),
                    InventoryMovementPublicId = table.Column<Guid>(type: "uuid", nullable: true),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovementLinks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Operators",
                schema: "rfid",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyPublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    FullName = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    DocumentId = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Operators", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Organizations",
                schema: "iam",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AccountType = table.Column<string>(type: "text", nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organizations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OutboxMessages",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Type = table.Column<string>(type: "text", nullable: false),
                    PayloadJson = table.Column<string>(type: "text", nullable: false),
                    Attempts = table.Column<int>(type: "integer", nullable: false),
                    OccurredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NextRetryAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeadLettered = table.Column<bool>(type: "boolean", nullable: false),
                    LastError = table.Column<string>(type: "text", nullable: true),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxMessages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PasswordResetTokens",
                schema: "iam",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    TokenHash = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ExpiresAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UsedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PasswordResetTokens", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Permissions",
                schema: "iam",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    ScopeType = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)3),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                schema: "iam",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    RequiredFeatureKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    IsSystem = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SubscriptionPlans",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    PriceAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    PriceCurrency = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    BillingCycle = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SeatLimit = table.Column<int>(type: "integer", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionPlans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tags",
                schema: "rfid",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyPublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    Epc = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ProductPublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TaxEntities",
                schema: "iam",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyPublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    TaxId = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaxEntities", x => x.Id);
                    table.UniqueConstraint("AK_TaxEntities_CompanyPublicId_Id", x => new { x.CompanyPublicId, x.Id });
                });

            migrationBuilder.CreateTable(
                name: "UnitOfMeasures",
                schema: "masterdata",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CanonicalCode = table.Column<string>(type: "text", nullable: false),
                    DisplayCode = table.Column<string>(type: "text", nullable: false),
                    Dimension = table.Column<string>(type: "text", nullable: false),
                    IsBaseUnit = table.Column<bool>(type: "boolean", nullable: false),
                    FactorToBase = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: false),
                    PrecisionScale = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: true),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnitOfMeasures", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserAuditLog",
                schema: "iam",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: true),
                    Action = table.Column<string>(type: "text", nullable: false),
                    EntityName = table.Column<string>(type: "text", nullable: false),
                    EntityId = table.Column<string>(type: "text", nullable: false),
                    ChangesJson = table.Column<string>(type: "text", nullable: false),
                    Source = table.Column<string>(type: "text", nullable: false),
                    OccurredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAuditLog", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserPasswordHistory",
                schema: "iam",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    PasswordSalt = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPasswordHistory", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                schema: "iam",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Email = table.Column<string>(type: "text", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    PasswordSalt = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowDefinitions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DocumentType = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowDefinitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowHistories",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WorkflowInstanceId = table.Column<long>(type: "bigint", nullable: false),
                    FromStateId = table.Column<long>(type: "bigint", nullable: true),
                    ToStateId = table.Column<long>(type: "bigint", nullable: true),
                    ActionType = table.Column<int>(type: "integer", nullable: false),
                    PerformedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    PerformedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Comment = table.Column<string>(type: "text", nullable: true),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowHistories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowInstances",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WorkflowDefinitionId = table.Column<long>(type: "bigint", nullable: false),
                    DocumentType = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DocumentId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CurrentStateId = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ContextDataJson = table.Column<string>(type: "text", nullable: true),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowInstances", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowTasks",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WorkflowInstanceId = table.Column<long>(type: "bigint", nullable: false),
                    WorkflowStateId = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    AssignedRole = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    AssignedUserId = table.Column<long>(type: "bigint", nullable: true),
                    CompletedAction = table.Column<int>(type: "integer", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowTasks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Subdivision",
                schema: "masterdata",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CountryId = table.Column<long>(type: "bigint", nullable: false),
                    Code = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Level = table.Column<short>(type: "smallint", nullable: false),
                    ParentSubdivisionId = table.Column<long>(type: "bigint", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subdivision", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Subdivision_Country_CountryId",
                        column: x => x.CountryId,
                        principalSchema: "masterdata",
                        principalTable: "Country",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Subdivision_Subdivision_ParentSubdivisionId",
                        column: x => x.ParentSubdivisionId,
                        principalSchema: "masterdata",
                        principalTable: "Subdivision",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DocumentLinks",
                schema: "documents",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyPublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    Url = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DocumentId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentLinks_Documents_DocumentId",
                        column: x => x.DocumentId,
                        principalSchema: "documents",
                        principalTable: "Documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DocumentVersions",
                schema: "documents",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyPublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    FileName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Size = table.Column<long>(type: "bigint", nullable: false),
                    Label = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    UploadedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DocumentId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentVersions_Documents_DocumentId",
                        column: x => x.DocumentId,
                        principalSchema: "documents",
                        principalTable: "Documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AccessSessions",
                schema: "rfid",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyPublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    UserPublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    WarehousePublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    AccessCardId = table.Column<string>(type: "text", nullable: true),
                    FaceTemplateId = table.Column<string>(type: "text", nullable: true),
                    NfcCardUid = table.Column<string>(type: "text", nullable: true),
                    DeviceCode = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EndedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    OperatorId = table.Column<long>(type: "bigint", nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccessSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccessSessions_Operators_OperatorId",
                        column: x => x.OperatorId,
                        principalSchema: "rfid",
                        principalTable: "Operators",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OperatorCredentials",
                schema: "rfid",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyPublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    OperatorId = table.Column<long>(type: "bigint", nullable: false),
                    FaceTemplateId = table.Column<string>(type: "text", nullable: true),
                    NfcCardUid = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OperatorCredentials", x => x.Id);
                    table.CheckConstraint("CK_OperatorCredentials_FaceOrNfc", "\"FaceTemplateId\" IS NOT NULL OR \"NfcCardUid\" IS NOT NULL");
                    table.ForeignKey(
                        name: "FK_OperatorCredentials_Operators_OperatorId",
                        column: x => x.OperatorId,
                        principalSchema: "rfid",
                        principalTable: "Operators",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Companies",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrganizationId = table.Column<long>(type: "bigint", nullable: false),
                    TaxEntityId = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Companies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Companies_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalSchema: "iam",
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CompanyLinks",
                schema: "iam",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrganizationId = table.Column<long>(type: "bigint", nullable: false),
                    CompanyPublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    AccessType = table.Column<string>(type: "text", nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanyLinks_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalSchema: "iam",
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RolePermissions",
                schema: "iam",
                columns: table => new
                {
                    RoleId = table.Column<long>(type: "bigint", nullable: false),
                    PermissionId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermissions", x => new { x.RoleId, x.PermissionId });
                    table.ForeignKey(
                        name: "FK_RolePermissions_Permissions_PermissionId",
                        column: x => x.PermissionId,
                        principalSchema: "iam",
                        principalTable: "Permissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RolePermissions_Roles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "iam",
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SubscriptionPlanFeatures",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PlanId = table.Column<long>(type: "bigint", nullable: false),
                    FeatureKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionPlanFeatures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubscriptionPlanFeatures_SubscriptionPlans_PlanId",
                        column: x => x.PlanId,
                        principalTable: "SubscriptionPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Subscriptions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyId = table.Column<long>(type: "bigint", nullable: false),
                    PlanId = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CancelledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Subscriptions_SubscriptionPlans_PlanId",
                        column: x => x.PlanId,
                        principalTable: "SubscriptionPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UnitOfMeasureExternalMappings",
                schema: "masterdata",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UnitOfMeasureId = table.Column<long>(type: "bigint", nullable: false),
                    Scheme = table.Column<string>(type: "text", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnitOfMeasureExternalMappings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UnitOfMeasureExternalMappings_UnitOfMeasures_UnitOfMeasureId",
                        column: x => x.UnitOfMeasureId,
                        principalSchema: "masterdata",
                        principalTable: "UnitOfMeasures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UnitOfMeasureTranslations",
                schema: "masterdata",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UnitOfMeasureId = table.Column<long>(type: "bigint", nullable: false),
                    Culture = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Symbol = table.Column<string>(type: "text", nullable: true),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnitOfMeasureTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UnitOfMeasureTranslations_UnitOfMeasures_UnitOfMeasureId",
                        column: x => x.UnitOfMeasureId,
                        principalSchema: "masterdata",
                        principalTable: "UnitOfMeasures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CompanyUsers",
                schema: "iam",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyPublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanyUsers_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "iam",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrganizationMembers",
                schema: "iam",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrganizationId = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    Role = table.Column<string>(type: "text", nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizationMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrganizationMembers_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalSchema: "iam",
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrganizationMembers_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "iam",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RoleAssignments",
                schema: "iam",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    RoleId = table.Column<long>(type: "bigint", nullable: false),
                    ScopeType = table.Column<short>(type: "smallint", nullable: false),
                    OrganizationId = table.Column<long>(type: "bigint", nullable: true),
                    CompanyPublicId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)1),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoleAssignments_Roles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "iam",
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RoleAssignments_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "iam",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserProfiles",
                schema: "iam",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    FirstName = table.Column<string>(type: "text", nullable: false),
                    LastName = table.Column<string>(type: "text", nullable: false),
                    DisplayName = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserProfiles_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "iam",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserSessions",
                schema: "iam",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    DeviceInfo = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    RefreshTokenHash = table.Column<string>(type: "text", nullable: false),
                    ReplacedByTokenHash = table.Column<string>(type: "text", nullable: true),
                    ReplacedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RevokedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastSeenAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserSessions_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "iam",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowStates",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WorkflowDefinitionId = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    IsInitial = table.Column<bool>(type: "boolean", nullable: false),
                    IsFinal = table.Column<bool>(type: "boolean", nullable: false),
                    RequiresApproval = table.Column<bool>(type: "boolean", nullable: false),
                    AssignedRole = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    AssignedUserId = table.Column<long>(type: "bigint", nullable: true),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowStates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowStates_WorkflowDefinitions_WorkflowDefinitionId",
                        column: x => x.WorkflowDefinitionId,
                        principalTable: "WorkflowDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "City",
                schema: "masterdata",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CountryId = table.Column<long>(type: "bigint", nullable: false),
                    SubdivisionId = table.Column<long>(type: "bigint", nullable: true),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    OfficialCode = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    PostalCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    IsCapital = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_City", x => x.Id);
                    table.ForeignKey(
                        name: "FK_City_Country_CountryId",
                        column: x => x.CountryId,
                        principalSchema: "masterdata",
                        principalTable: "Country",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_City_Subdivision_SubdivisionId",
                        column: x => x.SubdivisionId,
                        principalSchema: "masterdata",
                        principalTable: "Subdivision",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AccountingAutomationRules",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Trigger = table.Column<string>(type: "text", nullable: false),
                    DteDocumentType = table.Column<string>(type: "text", nullable: true),
                    SourceModule = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    SourceDocumentType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    CounterpartyTaxId = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    DebitAccountId = table.Column<long>(type: "bigint", nullable: false),
                    CreditAccountId = table.Column<long>(type: "bigint", nullable: false),
                    TaxCode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    ProposedBySystem = table.Column<bool>(type: "boolean", nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompanyId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountingAutomationRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountingAutomationRules_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AccountingPeriods",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyId = table.Column<long>(type: "bigint", nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    Month = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    ClosedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ClosedBy = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountingPeriods", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountingPeriods_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AccountsPayable",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SupplierId = table.Column<long>(type: "bigint", nullable: false),
                    PurchaseOrderId = table.Column<long>(type: "bigint", nullable: false),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalCurrency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    AmountDue = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    AmountDueCurrency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompanyId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountsPayable", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountsPayable_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AccountsPayables",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SupplierId = table.Column<long>(type: "bigint", nullable: false),
                    SourceType = table.Column<int>(type: "integer", nullable: false),
                    SourceId = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    TotalAmountAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalAmountCurrency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompanyId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountsPayables", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountsPayables_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AccountsReceivable",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CustomerId = table.Column<long>(type: "bigint", nullable: false),
                    SalesDocumentId = table.Column<long>(type: "bigint", nullable: false),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalCurrency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    AmountDue = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    AmountDueCurrency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompanyId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountsReceivable", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountsReceivable_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AccountsReceivables",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CustomerId = table.Column<long>(type: "bigint", nullable: false),
                    Origin = table.Column<int>(type: "integer", nullable: false),
                    SourceDocumentId = table.Column<long>(type: "bigint", nullable: false),
                    IssueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    OutstandingAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompanyId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountsReceivables", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountsReceivables_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AssetCategories",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    DepreciationMethod = table.Column<int>(type: "integer", nullable: false),
                    UsefulLifeMonths = table.Column<int>(type: "integer", nullable: false),
                    AssetAccountId = table.Column<long>(type: "bigint", nullable: false),
                    AccumulatedDepreciationAccountId = table.Column<long>(type: "bigint", nullable: false),
                    DepreciationExpenseAccountId = table.Column<long>(type: "bigint", nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompanyId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssetCategories_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BankAccounts",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    BankName = table.Column<string>(type: "text", nullable: false),
                    AccountNumber = table.Column<string>(type: "text", nullable: false),
                    Currency = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompanyId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankAccounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BankAccounts_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BankStatements",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BankAccountId = table.Column<long>(type: "bigint", nullable: false),
                    StartingBalance = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    EndingBalance = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    BankAccountName = table.Column<string>(type: "text", nullable: true),
                    StatementDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompanyId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankStatements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BankStatements_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CompanyAccountingSettings",
                columns: table => new
                {
                    CompanyId = table.Column<long>(type: "bigint", nullable: false),
                    BaseCurrencyCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    AccountsReceivableAccountId = table.Column<long>(type: "bigint", nullable: true),
                    AccountsPayableAccountId = table.Column<long>(type: "bigint", nullable: true),
                    SalesRevenueAccountId = table.Column<long>(type: "bigint", nullable: true),
                    VatDebitAccountId = table.Column<long>(type: "bigint", nullable: true),
                    VatCreditAccountId = table.Column<long>(type: "bigint", nullable: true),
                    CashAccountId = table.Column<long>(type: "bigint", nullable: true),
                    BankAccountId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyAccountingSettings", x => x.CompanyId);
                    table.ForeignKey(
                        name: "FK_CompanyAccountingSettings_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CompanyChartOfAccounts",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TemplateId = table.Column<long>(type: "bigint", nullable: true),
                    Code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    AccountType = table.Column<string>(type: "text", nullable: false),
                    ParentId = table.Column<long>(type: "bigint", nullable: true),
                    IsSystemRequired = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsLocked = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    IsPostable = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CurrencyCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    SystemRole = table.Column<string>(type: "text", nullable: true),
                    SourceTemplateAccountPublicId = table.Column<Guid>(type: "uuid", nullable: true),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompanyId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyChartOfAccounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanyChartOfAccounts_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CompanyCurrencies",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyId = table.Column<long>(type: "bigint", nullable: false),
                    CurrencyId = table.Column<long>(type: "bigint", nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyCurrencies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanyCurrencies_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CompanyCurrencies_Currency_CurrencyId",
                        column: x => x.CurrencyId,
                        principalSchema: "masterdata",
                        principalTable: "Currency",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CompanyFeatureAudits",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyId = table.Column<long>(type: "bigint", nullable: false),
                    FeaturePublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    Action = table.Column<string>(type: "text", nullable: false),
                    ChangedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ChangedByUserId = table.Column<long>(type: "bigint", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: true),
                    CorrelationId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyFeatureAudits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanyFeatureAudits_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CompanyFeatures",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FeaturePublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompanyId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyFeatures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanyFeatures_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CompanyUnitsOfMeasure",
                columns: table => new
                {
                    CompanyId = table.Column<long>(type: "bigint", nullable: false),
                    UnitOfMeasureId = table.Column<long>(type: "bigint", nullable: false),
                    Dimension = table.Column<string>(type: "text", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsDefaultForDimension = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DisplayNameOverride = table.Column<string>(type: "text", nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyUnitsOfMeasure", x => new { x.CompanyId, x.UnitOfMeasureId });
                    table.ForeignKey(
                        name: "FK_CompanyUnitsOfMeasure_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CompanyUnitsOfMeasure_UnitOfMeasures_UnitOfMeasureId",
                        column: x => x.UnitOfMeasureId,
                        principalSchema: "masterdata",
                        principalTable: "UnitOfMeasures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ControlledSubstanceLedgers",
                schema: "pharma",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LedgerCode = table.Column<string>(type: "text", nullable: false),
                    OpenedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompanyId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ControlledSubstanceLedgers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ControlledSubstanceLedgers_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ControlledSubstanceReportExports",
                schema: "pharma",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ExportedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Format = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    From = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    To = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExportedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompanyId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ControlledSubstanceReportExports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ControlledSubstanceReportExports_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CreditNotes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    InvoiceId = table.Column<long>(type: "bigint", nullable: false),
                    Number = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    IssuedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Reason = table.Column<string>(type: "text", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompanyId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CreditNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CreditNotes_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    TaxId = table.Column<string>(type: "text", nullable: true),
                    Type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompanyId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Customers_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DebitNotes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    InvoiceId = table.Column<long>(type: "bigint", nullable: false),
                    Number = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    IssuedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompanyId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DebitNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DebitNotes_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DeliveryConfirmations",
                schema: "pharma",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ReferenceType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ReferenceId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    DeliveredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Carrier = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ReceivedBy = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ConfirmedByUserId = table.Column<long>(type: "bigint", nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompanyId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryConfirmations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeliveryConfirmations_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DispatchConfirmations",
                schema: "wms",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ReferenceType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ReferenceId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    DeliveredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Carrier = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    DriverName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    VehiclePlate = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    ReceivedBy = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ConfirmedByUserId = table.Column<long>(type: "bigint", nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompanyId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DispatchConfirmations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DispatchConfirmations_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Dispenses",
                schema: "pharma",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PerformedByUserId = table.Column<long>(type: "bigint", nullable: false),
                    ConfirmedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompanyId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dispenses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Dispenses_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DteDocuments",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Folio = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    DocumentType = table.Column<string>(type: "text", nullable: false),
                    IssueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CounterpartyTaxId = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    NetAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TaxAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CurrencyCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    Source = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    StatusReason = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompanyId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DteDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DteDocuments_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExpirationAlertRules",
                schema: "pharma",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DaysBeforeExpiry = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompanyId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpirationAlertRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExpirationAlertRules_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GoodsReceipts",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SupplierId = table.Column<long>(type: "bigint", nullable: false),
                    PurchaseOrderId = table.Column<long>(type: "bigint", nullable: true),
                    ReceivedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompanyId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GoodsReceipts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GoodsReceipts_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InventoryMovements",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProductId = table.Column<long>(type: "bigint", nullable: false),
                    FromWarehouseId = table.Column<long>(type: "bigint", nullable: true),
                    ToWarehouseId = table.Column<long>(type: "bigint", nullable: true),
                    Quantity = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    MovementType = table.Column<int>(type: "integer", nullable: false),
                    ReferenceType = table.Column<string>(type: "text", nullable: false),
                    ReferenceId = table.Column<string>(type: "text", nullable: false),
                    Source = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    SourceId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Reason = table.Column<string>(type: "text", nullable: true),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompanyId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryMovements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventoryMovements_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Invoices",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Number = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CustomerId = table.Column<long>(type: "bigint", nullable: false),
                    IssueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompanyId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invoices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Invoices_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "JournalEntries",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    JournalId = table.Column<long>(type: "bigint", nullable: false),
                    EntryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PostingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PeriodId = table.Column<long>(type: "bigint", nullable: false),
                    Description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    SourceModule = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SourceDocumentId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SourceDocumentType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ReversesEntryId = table.Column<long>(type: "bigint", nullable: true),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: false),
                    PostedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PostedBy = table.Column<long>(type: "bigint", nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompanyId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JournalEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JournalEntries_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Journals",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompanyId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Journals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Journals_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OfficialControlledBookEntries",
                schema: "pharma",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProductId = table.Column<long>(type: "bigint", nullable: false),
                    BatchNumber = table.Column<string>(type: "text", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    MovementType = table.Column<int>(type: "integer", nullable: false),
                    ReferenceDocument = table.Column<string>(type: "text", nullable: false),
                    IspFolio = table.Column<string>(type: "text", nullable: true),
                    IspReason = table.Column<string>(type: "text", nullable: true),
                    IspOriginDestination = table.Column<string>(type: "text", nullable: true),
                    IspSupplierOrPatient = table.Column<string>(type: "text", nullable: true),
                    IspPrescriber = table.Column<string>(type: "text", nullable: true),
                    IspTaxReference = table.Column<string>(type: "text", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompanyId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OfficialControlledBookEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OfficialControlledBookEntries_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SupplierId = table.Column<long>(type: "bigint", nullable: false),
                    PurchaseOrderId = table.Column<long>(type: "bigint", nullable: true),
                    BankAccountId = table.Column<long>(type: "bigint", nullable: true),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    AppliedAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PaidAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Reference = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompanyId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Payments_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PharmacyCriticalAudits",
                schema: "pharma",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EntityName = table.Column<string>(type: "text", nullable: false),
                    EntityId = table.Column<string>(type: "text", nullable: false),
                    Action = table.Column<string>(type: "text", nullable: false),
                    PayloadJson = table.Column<string>(type: "text", nullable: false),
                    RecordedBy = table.Column<string>(type: "text", nullable: false),
                    OccurredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompanyId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PharmacyCriticalAudits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PharmacyCriticalAudits_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PharmaProductProfiles",
                schema: "pharma",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProductId = table.Column<long>(type: "bigint", nullable: false),
                    RequiresBatch = table.Column<bool>(type: "boolean", nullable: false),
                    RequiresExpiry = table.Column<bool>(type: "boolean", nullable: false),
                    SaleCondition = table.Column<string>(type: "text", nullable: false),
                    IsRegulated = table.Column<bool>(type: "boolean", nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompanyId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PharmaProductProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PharmaProductProfiles_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PriceListItems",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PriceListId = table.Column<long>(type: "bigint", nullable: false),
                    ProductId = table.Column<long>(type: "bigint", nullable: false),
                    UnitPriceAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    UnitPriceCurrency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompanyId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PriceListItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PriceListItems_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PriceLists",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompanyId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PriceLists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PriceLists_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PricingPolicies",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MarginPercent = table.Column<decimal>(type: "numeric(8,2)", precision: 8, scale: 2, nullable: false),
                    RoundingMode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    DecimalPlaces = table.Column<int>(type: "integer", nullable: true),
                    UpdatedByUserId = table.Column<long>(type: "bigint", nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompanyId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PricingPolicies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PricingPolicies_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductPharmaInfos",
                schema: "pharma",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProductId = table.Column<long>(type: "bigint", nullable: false),
                    Laboratory = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Presentation = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ActiveIngredient = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    IsBioequivalent = table.Column<bool>(type: "boolean", nullable: false),
                    CenabastCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Barcode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompanyId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductPharmaInfos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductPharmaInfos_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Sku = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Barcode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    IsStockable = table.Column<bool>(type: "boolean", nullable: false),
                    IsSellable = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsPurchasable = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    UnitOfMeasureId = table.Column<long>(type: "bigint", nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompanyId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Products_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Products_UnitOfMeasures_UnitOfMeasureId",
                        column: x => x.UnitOfMeasureId,
                        principalSchema: "masterdata",
                        principalTable: "UnitOfMeasures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseInvoiceIngestions",
                schema: "pharma",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SupplierId = table.Column<long>(type: "bigint", nullable: false),
                    PurchaseOrderId = table.Column<long>(type: "bigint", nullable: true),
                    InvoiceNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    IssuedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DueAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NetAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    NetCurrency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    TaxAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TaxCurrency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalCurrency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    QrPayload = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    GoodsReceiptId = table.Column<long>(type: "bigint", nullable: true),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompanyId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseInvoiceIngestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseInvoiceIngestions_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseOrders",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SupplierId = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Currency = table.Column<string>(type: "text", nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompanyId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseOrders_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseSuggestions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    SupplierSuggestedId = table.Column<long>(type: "bigint", nullable: true),
                    ConvertedPurchaseOrderId = table.Column<long>(type: "bigint", nullable: true),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompanyId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseSuggestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseSuggestions_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Receipts",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CustomerId = table.Column<long>(type: "bigint", nullable: false),
                    SalesDocumentId = table.Column<long>(type: "bigint", nullable: true),
                    BankAccountId = table.Column<long>(type: "bigint", nullable: true),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    AppliedAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ReceivedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Reference = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompanyId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Receipts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Receipts_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SalesDocuments",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Kind = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CustomerId = table.Column<long>(type: "bigint", nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompanyId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalesDocuments_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SalesQuotes",
                schema: "sales",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    QuoteNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CustomerId = table.Column<long>(type: "bigint", nullable: true),
                    CustomerName = table.Column<string>(type: "text", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    CurrencyCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    Subtotal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TaxTotal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedByUserId = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedByUserId = table.Column<long>(type: "bigint", nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompanyId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesQuotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalesQuotes_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StockLedgerEntries",
                schema: "pharma",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StockBatchId = table.Column<long>(type: "bigint", nullable: true),
                    ProductId = table.Column<long>(type: "bigint", nullable: false),
                    WarehouseId = table.Column<long>(type: "bigint", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    MovementType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ReferenceType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ReferenceId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PerformedByUserId = table.Column<long>(type: "bigint", nullable: true),
                    OccurredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompanyId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockLedgerEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StockLedgerEntries_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StockoutThresholds",
                schema: "pharma",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProductId = table.Column<long>(type: "bigint", nullable: false),
                    WarehouseId = table.Column<long>(type: "bigint", nullable: true),
                    ThresholdQuantity = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompanyId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockoutThresholds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StockoutThresholds_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Stocks",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProductId = table.Column<long>(type: "bigint", nullable: false),
                    WarehouseId = table.Column<long>(type: "bigint", nullable: false),
                    OnHandQuantity = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompanyId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stocks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Stocks_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Suppliers",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    TaxId = table.Column<string>(type: "text", nullable: true),
                    CountryId = table.Column<long>(type: "bigint", nullable: true),
                    DefaultCurrencyId = table.Column<long>(type: "bigint", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompanyId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Suppliers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Suppliers_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Suppliers_Country_CountryId",
                        column: x => x.CountryId,
                        principalSchema: "masterdata",
                        principalTable: "Country",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Suppliers_Currency_DefaultCurrencyId",
                        column: x => x.DefaultCurrencyId,
                        principalSchema: "masterdata",
                        principalTable: "Currency",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TaxBookEntries",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    Month = table.Column<int>(type: "integer", nullable: false),
                    BookType = table.Column<string>(type: "text", nullable: false),
                    DteDocumentId = table.Column<long>(type: "bigint", nullable: false),
                    Folio = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    DocumentType = table.Column<string>(type: "text", nullable: false),
                    IssueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CounterpartyTaxId = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    NetAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TaxAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CurrencyCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompanyId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaxBookEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaxBookEntries_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TaxDeclarations",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    Month = table.Column<int>(type: "integer", nullable: false),
                    DeclarationType = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    Payload = table.Column<string>(type: "jsonb", nullable: false),
                    ExternalReference = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    StatusMessage = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompanyId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaxDeclarations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaxDeclarations_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Taxes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Rate = table.Column<decimal>(type: "numeric(8,4)", precision: 8, scale: 4, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompanyId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Taxes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Taxes_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TaxGroups",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompanyId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaxGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaxGroups_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Warehouses",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompanyId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Warehouses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Warehouses_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SubscriptionBillingCycles",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SubscriptionId = table.Column<long>(type: "bigint", nullable: false),
                    CycleType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    StartsAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndsAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionBillingCycles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubscriptionBillingCycles_Subscriptions_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalTable: "Subscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SubscriptionSeatCounts",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SubscriptionId = table.Column<long>(type: "bigint", nullable: false),
                    MaxSeats = table.Column<int>(type: "integer", nullable: false),
                    UsedSeats = table.Column<int>(type: "integer", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionSeatCounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubscriptionSeatCounts_Subscriptions_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalTable: "Subscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SubscriptionTrials",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SubscriptionId = table.Column<long>(type: "bigint", nullable: false),
                    StartsAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndsAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionTrials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubscriptionTrials_Subscriptions_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalTable: "Subscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SubscriptionUsage",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SubscriptionId = table.Column<long>(type: "bigint", nullable: false),
                    Metric = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric", nullable: false),
                    RecordedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionUsage", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubscriptionUsage_Subscriptions_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalTable: "Subscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowTransitions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WorkflowDefinitionId = table.Column<long>(type: "bigint", nullable: false),
                    FromStateId = table.Column<long>(type: "bigint", nullable: false),
                    ToStateId = table.Column<long>(type: "bigint", nullable: false),
                    ActionType = table.Column<int>(type: "integer", nullable: false),
                    RuleJson = table.Column<string>(type: "text", nullable: true),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowTransitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowTransitions_WorkflowDefinitions_WorkflowDefinitionId",
                        column: x => x.WorkflowDefinitionId,
                        principalTable: "WorkflowDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkflowTransitions_WorkflowStates_FromStateId",
                        column: x => x.FromStateId,
                        principalTable: "WorkflowStates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkflowTransitions_WorkflowStates_ToStateId",
                        column: x => x.ToStateId,
                        principalTable: "WorkflowStates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Locality",
                schema: "masterdata",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CityId = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Locality", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Locality_City_CityId",
                        column: x => x.CityId,
                        principalSchema: "masterdata",
                        principalTable: "City",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PayableSchedules",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AccountsPayableId = table.Column<long>(type: "bigint", nullable: false),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    AmountAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    AmountCurrency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompanyId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayableSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PayableSchedules_AccountsPayables_AccountsPayableId",
                        column: x => x.AccountsPayableId,
                        principalTable: "AccountsPayables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PayableSchedules_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReceivableSchedules",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AccountsReceivableId = table.Column<long>(type: "bigint", nullable: false),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompanyId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReceivableSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReceivableSchedules_AccountsReceivables_AccountsReceivableId",
                        column: x => x.AccountsReceivableId,
                        principalTable: "AccountsReceivables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReceivableSchedules_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FixedAssets",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    CategoryId = table.Column<long>(type: "bigint", nullable: false),
                    AcquisitionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Cost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    SalvageValue = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    UsefulLifeMonths = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    DisposalDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompanyId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FixedAssets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FixedAssets_AssetCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "AssetCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FixedAssets_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BankTransactions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BankStatementId = table.Column<long>(type: "bigint", nullable: false),
                    TransactionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    Kind = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    Reference = table.Column<string>(type: "text", nullable: true),
                    MatchedPaymentId = table.Column<long>(type: "bigint", nullable: true),
                    MatchedReceiptId = table.Column<long>(type: "bigint", nullable: true),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompanyId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BankTransactions_BankStatements_BankStatementId",
                        column: x => x.BankStatementId,
                        principalTable: "BankStatements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BankTransactions_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DispenseLines",
                schema: "pharma",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DispenseId = table.Column<long>(type: "bigint", nullable: false),
                    ProductId = table.Column<long>(type: "bigint", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompanyId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DispenseLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DispenseLines_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DispenseLines_Dispenses_DispenseId",
                        column: x => x.DispenseId,
                        principalSchema: "pharma",
                        principalTable: "Dispenses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Prescriptions",
                schema: "pharma",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DispenseId = table.Column<long>(type: "bigint", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    IssuedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Patient = table.Column<string>(type: "text", nullable: false),
                    PatientIdentification = table.Column<string>(type: "text", nullable: true),
                    Doctor = table.Column<string>(type: "text", nullable: false),
                    DoctorIdentification = table.Column<string>(type: "text", nullable: true),
                    Folio = table.Column<string>(type: "text", nullable: false),
                    AttachmentUrl = table.Column<string>(type: "text", nullable: true),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompanyId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prescriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Prescriptions_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Prescriptions_Dispenses_DispenseId",
                        column: x => x.DispenseId,
                        principalSchema: "pharma",
                        principalTable: "Dispenses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GoodsReceiptLines",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GoodsReceiptId = table.Column<long>(type: "bigint", nullable: false),
                    ProductId = table.Column<long>(type: "bigint", nullable: false),
                    WarehouseId = table.Column<long>(type: "bigint", nullable: false),
                    ReceivedQty = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompanyId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GoodsReceiptLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GoodsReceiptLines_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GoodsReceiptLines_GoodsReceipts_GoodsReceiptId",
                        column: x => x.GoodsReceiptId,
                        principalTable: "GoodsReceipts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InvoiceLines",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    InvoiceId = table.Column<long>(type: "bigint", nullable: false),
                    ProductId = table.Column<long>(type: "bigint", nullable: false),
                    Qty = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    UnitPriceAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    UnitPriceCurrency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompanyId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvoiceLines_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InvoiceLines_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "JournalEntryLines",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    JournalEntryId = table.Column<long>(type: "bigint", nullable: false),
                    AccountId = table.Column<long>(type: "bigint", nullable: false),
                    Debit = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Credit = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CurrencyCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    FxRate = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    AmountInBaseCurrency = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CostCenterId = table.Column<long>(type: "bigint", nullable: true),
                    ProjectId = table.Column<long>(type: "bigint", nullable: true),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompanyId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JournalEntryLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JournalEntryLines_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JournalEntryLines_JournalEntries_JournalEntryId",
                        column: x => x.JournalEntryId,
                        principalTable: "JournalEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ControlledSubstanceLedgerAudits",
                schema: "pharma",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OfficialControlledBookEntryId = table.Column<long>(type: "bigint", nullable: false),
                    Action = table.Column<int>(type: "integer", nullable: false),
                    RecordedBy = table.Column<string>(type: "text", nullable: false),
                    OccurredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PayloadJson = table.Column<string>(type: "text", nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompanyId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ControlledSubstanceLedgerAudits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ControlledSubstanceLedgerAudits_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ControlledSubstanceLedgerAudits_OfficialControlledBookEntri~",
                        column: x => x.OfficialControlledBookEntryId,
                        principalSchema: "pharma",
                        principalTable: "OfficialControlledBookEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductUnitConversions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProductId = table.Column<long>(type: "bigint", nullable: false),
                    FromUnitOfMeasureId = table.Column<long>(type: "bigint", nullable: false),
                    ToUnitOfMeasureId = table.Column<long>(type: "bigint", nullable: false),
                    Factor = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: false),
                    RoundingMode = table.Column<string>(type: "text", nullable: true),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductUnitConversions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductUnitConversions_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductUnitConversions_UnitOfMeasures_FromUnitOfMeasureId",
                        column: x => x.FromUnitOfMeasureId,
                        principalSchema: "masterdata",
                        principalTable: "UnitOfMeasures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductUnitConversions_UnitOfMeasures_ToUnitOfMeasureId",
                        column: x => x.ToUnitOfMeasureId,
                        principalSchema: "masterdata",
                        principalTable: "UnitOfMeasures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseInvoiceIngestionLines",
                schema: "pharma",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PurchaseInvoiceIngestionId = table.Column<long>(type: "bigint", nullable: false),
                    ProductId = table.Column<long>(type: "bigint", nullable: false),
                    WarehouseId = table.Column<long>(type: "bigint", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    UnitPriceAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    UnitPriceCurrency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    BatchNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    InboundPresentation = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompanyId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseInvoiceIngestionLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseInvoiceIngestionLines_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseInvoiceIngestionLines_PurchaseInvoiceIngestions_Pur~",
                        column: x => x.PurchaseInvoiceIngestionId,
                        principalSchema: "pharma",
                        principalTable: "PurchaseInvoiceIngestions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseOrderLines",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PurchaseOrderId = table.Column<long>(type: "bigint", nullable: false),
                    ProductId = table.Column<long>(type: "bigint", nullable: false),
                    OrderedQty = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    UnitPriceAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    UnitPriceCurrency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    TaxGroupId = table.Column<long>(type: "bigint", nullable: true),
                    NetAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    NetCurrency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    TaxAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TaxCurrency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalCurrency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompanyId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseOrderLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseOrderLines_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseOrderLines_PurchaseOrders_PurchaseOrderId",
                        column: x => x.PurchaseOrderId,
                        principalTable: "PurchaseOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseSuggestionLines",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PurchaseSuggestionId = table.Column<long>(type: "bigint", nullable: false),
                    ProductId = table.Column<long>(type: "bigint", nullable: false),
                    QtySuggested = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    SupplierSuggestedId = table.Column<long>(type: "bigint", nullable: true),
                    Reason = table.Column<int>(type: "integer", nullable: false),
                    WarehouseId = table.Column<long>(type: "bigint", nullable: true),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompanyId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseSuggestionLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseSuggestionLines_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseSuggestionLines_PurchaseSuggestions_PurchaseSuggest~",
                        column: x => x.PurchaseSuggestionId,
                        principalTable: "PurchaseSuggestions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SalesDocumentLines",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SalesDocumentId = table.Column<long>(type: "bigint", nullable: false),
                    ProductId = table.Column<long>(type: "bigint", nullable: false),
                    Qty = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    UnitPriceAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    UnitPriceCurrency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    TaxGroupId = table.Column<long>(type: "bigint", nullable: true),
                    NetAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    NetCurrency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    TaxAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TaxCurrency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalCurrency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompanyId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesDocumentLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalesDocumentLines_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SalesDocumentLines_SalesDocuments_SalesDocumentId",
                        column: x => x.SalesDocumentId,
                        principalTable: "SalesDocuments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SalesQuoteLines",
                schema: "sales",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    QuoteId = table.Column<long>(type: "bigint", nullable: false),
                    LineNumber = table.Column<int>(type: "integer", nullable: false),
                    ProductId = table.Column<long>(type: "bigint", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    LineTotal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TaxRate = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompanyId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesQuoteLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalesQuoteLines_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SalesQuoteLines_SalesQuotes_QuoteId",
                        column: x => x.QuoteId,
                        principalSchema: "sales",
                        principalTable: "SalesQuotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TaxRules",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TaxGroupId = table.Column<long>(type: "bigint", nullable: false),
                    TaxId = table.Column<long>(type: "bigint", nullable: false),
                    Sequence = table.Column<int>(type: "integer", nullable: false),
                    Rate = table.Column<decimal>(type: "numeric(8,4)", precision: 8, scale: 4, nullable: false),
                    IsCompound = table.Column<bool>(type: "boolean", nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompanyId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaxRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaxRules_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TaxRules_TaxGroups_TaxGroupId",
                        column: x => x.TaxGroupId,
                        principalTable: "TaxGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TaxRules_Taxes_TaxId",
                        column: x => x.TaxId,
                        principalTable: "Taxes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WarehouseLocations",
                schema: "wms",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WarehouseId = table.Column<long>(type: "bigint", nullable: false),
                    Aisle = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Rack = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Side = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    Slot = table.Column<int>(type: "integer", nullable: false),
                    Capacity = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    IsPalletSlot = table.Column<bool>(type: "boolean", nullable: false),
                    IsStackable = table.Column<bool>(type: "boolean", nullable: false),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompanyId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WarehouseLocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WarehouseLocations_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WarehouseLocations_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WarehouseOperations",
                schema: "wms",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WarehouseId = table.Column<long>(type: "bigint", nullable: false),
                    OperationType = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    SourceLocationId = table.Column<long>(type: "bigint", nullable: true),
                    DestinationLocationId = table.Column<long>(type: "bigint", nullable: true),
                    ReferenceType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ReferenceId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    AssignedToUserId = table.Column<long>(type: "bigint", nullable: true),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompanyId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WarehouseOperations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WarehouseOperations_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WarehouseOperations_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Address",
                schema: "masterdata",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CountryId = table.Column<long>(type: "bigint", nullable: false),
                    Level1SubdivisionId = table.Column<long>(type: "bigint", nullable: true),
                    Level2SubdivisionId = table.Column<long>(type: "bigint", nullable: true),
                    CityId = table.Column<long>(type: "bigint", nullable: true),
                    LocalityId = table.Column<long>(type: "bigint", nullable: true),
                    Street = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    Number = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Unit = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    PostalCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Notes = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    GeoLat = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    GeoLng = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Address", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Address_City_CityId",
                        column: x => x.CityId,
                        principalSchema: "masterdata",
                        principalTable: "City",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Address_Country_CountryId",
                        column: x => x.CountryId,
                        principalSchema: "masterdata",
                        principalTable: "Country",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Address_Locality_LocalityId",
                        column: x => x.LocalityId,
                        principalSchema: "masterdata",
                        principalTable: "Locality",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Address_Subdivision_Level1SubdivisionId",
                        column: x => x.Level1SubdivisionId,
                        principalSchema: "masterdata",
                        principalTable: "Subdivision",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Address_Subdivision_Level2SubdivisionId",
                        column: x => x.Level2SubdivisionId,
                        principalSchema: "masterdata",
                        principalTable: "Subdivision",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DepreciationSchedules",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FixedAssetId = table.Column<long>(type: "bigint", nullable: false),
                    Method = table.Column<int>(type: "integer", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UsefulLifeMonths = table.Column<int>(type: "integer", nullable: false),
                    TotalDepreciableAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompanyId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DepreciationSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DepreciationSchedules_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DepreciationSchedules_FixedAssets_FixedAssetId",
                        column: x => x.FixedAssetId,
                        principalTable: "FixedAssets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PrescriptionItems",
                schema: "pharma",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PrescriptionId = table.Column<long>(type: "bigint", nullable: false),
                    DispenseLineId = table.Column<long>(type: "bigint", nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompanyId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrescriptionItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PrescriptionItems_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PrescriptionItems_DispenseLines_DispenseLineId",
                        column: x => x.DispenseLineId,
                        principalSchema: "pharma",
                        principalTable: "DispenseLines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PrescriptionItems_Prescriptions_PrescriptionId",
                        column: x => x.PrescriptionId,
                        principalSchema: "pharma",
                        principalTable: "Prescriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StockBatches",
                schema: "pharma",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProductId = table.Column<long>(type: "bigint", nullable: false),
                    WarehouseId = table.Column<long>(type: "bigint", nullable: false),
                    WarehouseLocationId = table.Column<long>(type: "bigint", nullable: true),
                    BatchNumber = table.Column<string>(type: "text", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    QuantityOnHand = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    HealthStatus = table.Column<int>(type: "integer", nullable: false),
                    RecallReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    WasteReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompanyId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockBatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StockBatches_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StockBatches_WarehouseLocations_WarehouseLocationId",
                        column: x => x.WarehouseLocationId,
                        principalSchema: "wms",
                        principalTable: "WarehouseLocations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DepreciationScheduleLines",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DepreciationScheduleId = table.Column<long>(type: "bigint", nullable: false),
                    PeriodDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    IsPosted = table.Column<bool>(type: "boolean", nullable: false),
                    PostedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompanyId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DepreciationScheduleLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DepreciationScheduleLines_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DepreciationScheduleLines_DepreciationSchedules_Depreciatio~",
                        column: x => x.DepreciationScheduleId,
                        principalTable: "DepreciationSchedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BlockHolds",
                schema: "pharma",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StockBatchId = table.Column<long>(type: "bigint", nullable: false),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CreatedByUserId = table.Column<long>(type: "bigint", nullable: false),
                    ReleasedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReleasedByUserId = table.Column<long>(type: "bigint", nullable: true),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompanyId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlockHolds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BlockHolds_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BlockHolds_StockBatches_StockBatchId",
                        column: x => x.StockBatchId,
                        principalSchema: "pharma",
                        principalTable: "StockBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MovementBatchAllocations",
                schema: "pharma",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MovementId = table.Column<long>(type: "bigint", nullable: false),
                    StockBatchId = table.Column<long>(type: "bigint", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompanyId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovementBatchAllocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MovementBatchAllocations_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MovementBatchAllocations_InventoryMovements_MovementId",
                        column: x => x.MovementId,
                        principalTable: "InventoryMovements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MovementBatchAllocations_StockBatches_StockBatchId",
                        column: x => x.StockBatchId,
                        principalSchema: "pharma",
                        principalTable: "StockBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "QuarantineHolds",
                schema: "pharma",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StockBatchId = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CreatedByUserId = table.Column<long>(type: "bigint", nullable: false),
                    ReleasedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReleasedByUserId = table.Column<long>(type: "bigint", nullable: true),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompanyId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuarantineHolds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuarantineHolds_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QuarantineHolds_StockBatches_StockBatchId",
                        column: x => x.StockBatchId,
                        principalSchema: "pharma",
                        principalTable: "StockBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Recalls",
                schema: "pharma",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StockBatchId = table.Column<long>(type: "bigint", nullable: false),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    ReportedByUserId = table.Column<long>(type: "bigint", nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompanyId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recalls", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Recalls_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Recalls_StockBatches_StockBatchId",
                        column: x => x.StockBatchId,
                        principalSchema: "pharma",
                        principalTable: "StockBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Wastes",
                schema: "pharma",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StockBatchId = table.Column<long>(type: "bigint", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ReportedByUserId = table.Column<long>(type: "bigint", nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompanyId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wastes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Wastes_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Wastes_StockBatches_StockBatchId",
                        column: x => x.StockBatchId,
                        principalSchema: "pharma",
                        principalTable: "StockBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PharmacyTraceEvents",
                schema: "pharma",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MovementId = table.Column<long>(type: "bigint", nullable: true),
                    MovementBatchAllocationId = table.Column<long>(type: "bigint", nullable: true),
                    StockBatchId = table.Column<long>(type: "bigint", nullable: false),
                    ProductId = table.Column<long>(type: "bigint", nullable: false),
                    WarehouseId = table.Column<long>(type: "bigint", nullable: false),
                    BatchNumber = table.Column<string>(type: "text", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Quantity = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    MovementType = table.Column<int>(type: "integer", nullable: true),
                    ReferenceType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ReferenceId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SupplierId = table.Column<long>(type: "bigint", nullable: true),
                    CustomerId = table.Column<long>(type: "bigint", nullable: true),
                    PatientReference = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompanyId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PharmacyTraceEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PharmacyTraceEvents_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PharmacyTraceEvents_InventoryMovements_MovementId",
                        column: x => x.MovementId,
                        principalTable: "InventoryMovements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PharmacyTraceEvents_MovementBatchAllocations_MovementBatchA~",
                        column: x => x.MovementBatchAllocationId,
                        principalSchema: "pharma",
                        principalTable: "MovementBatchAllocations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PharmacyTraceEvents_StockBatches_StockBatchId",
                        column: x => x.StockBatchId,
                        principalSchema: "pharma",
                        principalTable: "StockBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccessSessions_CompanyPublicId_DeviceCode_StartedAt",
                schema: "rfid",
                table: "AccessSessions",
                columns: new[] { "CompanyPublicId", "DeviceCode", "StartedAt" },
                descending: new[] { false, false, true });

            migrationBuilder.CreateIndex(
                name: "IX_AccessSessions_CompanyPublicId_OperatorId_StartedAt",
                schema: "rfid",
                table: "AccessSessions",
                columns: new[] { "CompanyPublicId", "OperatorId", "StartedAt" },
                descending: new[] { false, false, true });

            migrationBuilder.CreateIndex(
                name: "IX_AccessSessions_CompanyPublicId_SessionId",
                schema: "rfid",
                table: "AccessSessions",
                columns: new[] { "CompanyPublicId", "SessionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AccessSessions_CompanyPublicId_StartedAt",
                schema: "rfid",
                table: "AccessSessions",
                columns: new[] { "CompanyPublicId", "StartedAt" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_AccessSessions_CompanyPublicId_Status",
                schema: "rfid",
                table: "AccessSessions",
                columns: new[] { "CompanyPublicId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_AccessSessions_CompanyPublicId_WarehousePublicId_StartedAt",
                schema: "rfid",
                table: "AccessSessions",
                columns: new[] { "CompanyPublicId", "WarehousePublicId", "StartedAt" },
                descending: new[] { false, false, true });

            migrationBuilder.CreateIndex(
                name: "IX_AccessSessions_OperatorId",
                schema: "rfid",
                table: "AccessSessions",
                column: "OperatorId");

            migrationBuilder.CreateIndex(
                name: "IX_AccessSessions_PublicId",
                schema: "rfid",
                table: "AccessSessions",
                column: "PublicId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AccountingAutomationRules_CompanyId_Trigger",
                table: "AccountingAutomationRules",
                columns: new[] { "CompanyId", "Trigger" });

            migrationBuilder.CreateIndex(
                name: "IX_AccountingPeriods_CompanyId_Year_Month",
                table: "AccountingPeriods",
                columns: new[] { "CompanyId", "Year", "Month" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AccountsPayable_CompanyId",
                table: "AccountsPayable",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountsPayables_CompanyId",
                table: "AccountsPayables",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountsReceivable_CompanyId",
                table: "AccountsReceivable",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountsReceivables_CompanyId",
                table: "AccountsReceivables",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Address_CityId",
                schema: "masterdata",
                table: "Address",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_Address_CountryId",
                schema: "masterdata",
                table: "Address",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_Address_Level1SubdivisionId",
                schema: "masterdata",
                table: "Address",
                column: "Level1SubdivisionId");

            migrationBuilder.CreateIndex(
                name: "IX_Address_Level2SubdivisionId",
                schema: "masterdata",
                table: "Address",
                column: "Level2SubdivisionId");

            migrationBuilder.CreateIndex(
                name: "IX_Address_LocalityId",
                schema: "masterdata",
                table: "Address",
                column: "LocalityId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetCategories_CompanyId_Name",
                table: "AssetCategories",
                columns: new[] { "CompanyId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_OccurredAt",
                table: "AuditLogs",
                column: "OccurredAt");

            migrationBuilder.CreateIndex(
                name: "IX_BankAccounts_CompanyId",
                table: "BankAccounts",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_BankStatements_CompanyId",
                table: "BankStatements",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_BankTransactions_BankStatementId",
                table: "BankTransactions",
                column: "BankStatementId");

            migrationBuilder.CreateIndex(
                name: "IX_BankTransactions_CompanyId",
                table: "BankTransactions",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_BlockHolds_CompanyId",
                schema: "pharma",
                table: "BlockHolds",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_BlockHolds_StockBatchId",
                schema: "pharma",
                table: "BlockHolds",
                column: "StockBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_ChartOfAccountsTemplates_Version_Code",
                schema: "masterdata",
                table: "ChartOfAccountsTemplates",
                columns: new[] { "Version", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChartOfAccountsTemplates_Version_SystemRole",
                schema: "masterdata",
                table: "ChartOfAccountsTemplates",
                columns: new[] { "Version", "SystemRole" },
                unique: true,
                filter: "\"SystemRole\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_City_CountryId",
                schema: "masterdata",
                table: "City",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_City_CountryId_Name",
                schema: "masterdata",
                table: "City",
                columns: new[] { "CountryId", "Name" },
                unique: true,
                filter: "\"SubdivisionId\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_City_CountryId_OfficialCode",
                schema: "masterdata",
                table: "City",
                columns: new[] { "CountryId", "OfficialCode" },
                unique: true,
                filter: "\"OfficialCode\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_City_CountryId_SubdivisionId_Name",
                schema: "masterdata",
                table: "City",
                columns: new[] { "CountryId", "SubdivisionId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_City_OfficialCode",
                schema: "masterdata",
                table: "City",
                column: "OfficialCode");

            migrationBuilder.CreateIndex(
                name: "IX_City_SubdivisionId",
                schema: "masterdata",
                table: "City",
                column: "SubdivisionId");

            migrationBuilder.CreateIndex(
                name: "IX_Companies_OrganizationId_Name",
                table: "Companies",
                columns: new[] { "OrganizationId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Companies_TaxEntityId",
                table: "Companies",
                column: "TaxEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyChartOfAccounts_CompanyId_AccountType",
                table: "CompanyChartOfAccounts",
                columns: new[] { "CompanyId", "AccountType" });

            migrationBuilder.CreateIndex(
                name: "IX_CompanyChartOfAccounts_CompanyId_Code",
                table: "CompanyChartOfAccounts",
                columns: new[] { "CompanyId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CompanyChartOfAccounts_CompanyId_ParentId",
                table: "CompanyChartOfAccounts",
                columns: new[] { "CompanyId", "ParentId" });

            migrationBuilder.CreateIndex(
                name: "IX_CompanyChartOfAccounts_CompanyId_SystemRole",
                table: "CompanyChartOfAccounts",
                columns: new[] { "CompanyId", "SystemRole" },
                unique: true,
                filter: "\"SystemRole\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyCurrencies_CompanyId",
                schema: "public",
                table: "CompanyCurrencies",
                column: "CompanyId",
                unique: true,
                filter: "\"IsDefault\" = true");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyCurrencies_CompanyId_CurrencyId",
                schema: "public",
                table: "CompanyCurrencies",
                columns: new[] { "CompanyId", "CurrencyId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CompanyCurrencies_CurrencyId",
                schema: "public",
                table: "CompanyCurrencies",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyFeatureAudits_CompanyId",
                schema: "public",
                table: "CompanyFeatureAudits",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyFeatures_CompanyId_FeaturePublicId",
                schema: "public",
                table: "CompanyFeatures",
                columns: new[] { "CompanyId", "FeaturePublicId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CompanyLinks_OrganizationId_CompanyPublicId",
                schema: "iam",
                table: "CompanyLinks",
                columns: new[] { "OrganizationId", "CompanyPublicId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CompanyUnitsOfMeasure_CompanyId_Dimension",
                table: "CompanyUnitsOfMeasure",
                columns: new[] { "CompanyId", "Dimension" },
                unique: true,
                filter: "\"IsDefaultForDimension\" = TRUE");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyUnitsOfMeasure_CompanyId_IsEnabled",
                table: "CompanyUnitsOfMeasure",
                columns: new[] { "CompanyId", "IsEnabled" });

            migrationBuilder.CreateIndex(
                name: "IX_CompanyUnitsOfMeasure_UnitOfMeasureId",
                table: "CompanyUnitsOfMeasure",
                column: "UnitOfMeasureId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyUsers_CompanyPublicId_UserId",
                schema: "iam",
                table: "CompanyUsers",
                columns: new[] { "CompanyPublicId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CompanyUsers_UserId",
                schema: "iam",
                table: "CompanyUsers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ControlledSubstanceLedgerAudits_CompanyId",
                schema: "pharma",
                table: "ControlledSubstanceLedgerAudits",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_ControlledSubstanceLedgerAudits_OfficialControlledBookEntry~",
                schema: "pharma",
                table: "ControlledSubstanceLedgerAudits",
                column: "OfficialControlledBookEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_ControlledSubstanceLedgers_CompanyId",
                schema: "pharma",
                table: "ControlledSubstanceLedgers",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_ControlledSubstanceReportExports_CompanyId_ExportedAt",
                schema: "pharma",
                table: "ControlledSubstanceReportExports",
                columns: new[] { "CompanyId", "ExportedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Country_Iso2",
                schema: "masterdata",
                table: "Country",
                column: "Iso2",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Country_Iso3",
                schema: "masterdata",
                table: "Country",
                column: "Iso3",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Country_Name",
                schema: "masterdata",
                table: "Country",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_CreditNotes_CompanyId_Number",
                table: "CreditNotes",
                columns: new[] { "CompanyId", "Number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Currency_Code",
                schema: "masterdata",
                table: "Currency",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Currency_NumericCode",
                schema: "masterdata",
                table: "Currency",
                column: "NumericCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customers_CompanyId",
                table: "Customers",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_DebitNotes_CompanyId_Number",
                table: "DebitNotes",
                columns: new[] { "CompanyId", "Number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryConfirmations_CompanyId_ReferenceType_ReferenceId",
                schema: "pharma",
                table: "DeliveryConfirmations",
                columns: new[] { "CompanyId", "ReferenceType", "ReferenceId" });

            migrationBuilder.CreateIndex(
                name: "IX_DepreciationScheduleLines_CompanyId",
                table: "DepreciationScheduleLines",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_DepreciationScheduleLines_DepreciationScheduleId",
                table: "DepreciationScheduleLines",
                column: "DepreciationScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_DepreciationSchedules_CompanyId_FixedAssetId",
                table: "DepreciationSchedules",
                columns: new[] { "CompanyId", "FixedAssetId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DepreciationSchedules_FixedAssetId",
                table: "DepreciationSchedules",
                column: "FixedAssetId");

            migrationBuilder.CreateIndex(
                name: "IX_DispatchConfirmations_CompanyId_ReferenceType_ReferenceId",
                schema: "wms",
                table: "DispatchConfirmations",
                columns: new[] { "CompanyId", "ReferenceType", "ReferenceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DispenseLines_CompanyId",
                schema: "pharma",
                table: "DispenseLines",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_DispenseLines_DispenseId",
                schema: "pharma",
                table: "DispenseLines",
                column: "DispenseId");

            migrationBuilder.CreateIndex(
                name: "IX_Dispenses_CompanyId",
                schema: "pharma",
                table: "Dispenses",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentLinks_CompanyPublicId_CreatedAt",
                schema: "documents",
                table: "DocumentLinks",
                columns: new[] { "CompanyPublicId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentLinks_DocumentId",
                schema: "documents",
                table: "DocumentLinks",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_CompanyPublicId_CreatedAt",
                schema: "documents",
                table: "Documents",
                columns: new[] { "CompanyPublicId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentSearchEntries_CompanyId_DocumentType_DocumentId",
                table: "DocumentSearchEntries",
                columns: new[] { "CompanyId", "DocumentType", "DocumentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DocumentSearchEntries_Metadata",
                table: "DocumentSearchEntries",
                column: "Metadata")
                .Annotation("Npgsql:IndexMethod", "GIN");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentSearchEntries_SearchVector",
                table: "DocumentSearchEntries",
                column: "SearchVector")
                .Annotation("Npgsql:IndexMethod", "GIN");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentVersions_CompanyPublicId_UploadedAt",
                schema: "documents",
                table: "DocumentVersions",
                columns: new[] { "CompanyPublicId", "UploadedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentVersions_DocumentId",
                schema: "documents",
                table: "DocumentVersions",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_DteDocuments_CompanyId_Folio_DocumentType",
                table: "DteDocuments",
                columns: new[] { "CompanyId", "Folio", "DocumentType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DteDocuments_CompanyId_IssueDate",
                table: "DteDocuments",
                columns: new[] { "CompanyId", "IssueDate" });

            migrationBuilder.CreateIndex(
                name: "IX_EventInbox_CompanyPublicId_EventId",
                schema: "rfid",
                table: "EventInbox",
                columns: new[] { "CompanyPublicId", "EventId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EventInbox_CompanyPublicId_OccurredAt",
                schema: "rfid",
                table: "EventInbox",
                columns: new[] { "CompanyPublicId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ExpirationAlertRules_CompanyId",
                schema: "pharma",
                table: "ExpirationAlertRules",
                column: "CompanyId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FeatureCatalog_Code",
                schema: "masterdata",
                table: "FeatureCatalog",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FeatureCatalog_PublicId",
                schema: "masterdata",
                table: "FeatureCatalog",
                column: "PublicId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FixedAssets_CategoryId",
                table: "FixedAssets",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_FixedAssets_CompanyId_Status",
                table: "FixedAssets",
                columns: new[] { "CompanyId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_GoodsReceiptLines_CompanyId",
                table: "GoodsReceiptLines",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_GoodsReceiptLines_GoodsReceiptId",
                table: "GoodsReceiptLines",
                column: "GoodsReceiptId");

            migrationBuilder.CreateIndex(
                name: "IX_GoodsReceipts_CompanyId",
                table: "GoodsReceipts",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryMovements_CompanyId_MovementType_CreatedAt",
                table: "InventoryMovements",
                columns: new[] { "CompanyId", "MovementType", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_InventoryMovements_CompanyId_ProductId_CreatedAt",
                table: "InventoryMovements",
                columns: new[] { "CompanyId", "ProductId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_InventoryMovements_CompanyId_Source_SourceId",
                table: "InventoryMovements",
                columns: new[] { "CompanyId", "Source", "SourceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceLines_CompanyId",
                table: "InvoiceLines",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceLines_InvoiceId",
                table: "InvoiceLines",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_CompanyId_Number",
                table: "Invoices",
                columns: new[] { "CompanyId", "Number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntries_CompanyId_SourceModule_SourceDocumentId_Sour~",
                table: "JournalEntries",
                columns: new[] { "CompanyId", "SourceModule", "SourceDocumentId", "SourceDocumentType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntryLines_CompanyId",
                table: "JournalEntryLines",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntryLines_JournalEntryId",
                table: "JournalEntryLines",
                column: "JournalEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_Journals_CompanyId_Code",
                table: "Journals",
                columns: new[] { "CompanyId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Locality_CityId",
                schema: "masterdata",
                table: "Locality",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_Locality_CityId_Name",
                schema: "masterdata",
                table: "Locality",
                columns: new[] { "CityId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MovementBatchAllocations_CompanyId",
                schema: "pharma",
                table: "MovementBatchAllocations",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_MovementBatchAllocations_MovementId",
                schema: "pharma",
                table: "MovementBatchAllocations",
                column: "MovementId");

            migrationBuilder.CreateIndex(
                name: "IX_MovementBatchAllocations_StockBatchId",
                schema: "pharma",
                table: "MovementBatchAllocations",
                column: "StockBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_OfficialControlledBookEntries_CompanyId_Date",
                schema: "pharma",
                table: "OfficialControlledBookEntries",
                columns: new[] { "CompanyId", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_OfficialControlledBookEntries_CompanyId_ProductId_Date",
                schema: "pharma",
                table: "OfficialControlledBookEntries",
                columns: new[] { "CompanyId", "ProductId", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_OperatorCredentials_CompanyPublicId_FaceTemplateId",
                schema: "rfid",
                table: "OperatorCredentials",
                columns: new[] { "CompanyPublicId", "FaceTemplateId" },
                unique: true,
                filter: "\"FaceTemplateId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_OperatorCredentials_CompanyPublicId_NfcCardUid",
                schema: "rfid",
                table: "OperatorCredentials",
                columns: new[] { "CompanyPublicId", "NfcCardUid" },
                unique: true,
                filter: "\"NfcCardUid\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_OperatorCredentials_OperatorId",
                schema: "rfid",
                table: "OperatorCredentials",
                column: "OperatorId");

            migrationBuilder.CreateIndex(
                name: "IX_OperatorCredentials_PublicId",
                schema: "rfid",
                table: "OperatorCredentials",
                column: "PublicId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Operators_CompanyPublicId_DocumentId",
                schema: "rfid",
                table: "Operators",
                columns: new[] { "CompanyPublicId", "DocumentId" },
                unique: true,
                filter: "\"DocumentId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Operators_CompanyPublicId_FullName",
                schema: "rfid",
                table: "Operators",
                columns: new[] { "CompanyPublicId", "FullName" });

            migrationBuilder.CreateIndex(
                name: "IX_Operators_PublicId",
                schema: "rfid",
                table: "Operators",
                column: "PublicId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationMembers_OrganizationId_UserId",
                schema: "iam",
                table: "OrganizationMembers",
                columns: new[] { "OrganizationId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationMembers_UserId",
                schema: "iam",
                table: "OrganizationMembers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_AccountType",
                schema: "iam",
                table: "Organizations",
                column: "AccountType");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_OccurredAt_Desc",
                table: "OutboxMessages",
                column: "OccurredAt",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_ProcessedAt",
                table: "OutboxMessages",
                column: "ProcessedAt");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_Type_OccurredAt_Desc",
                table: "OutboxMessages",
                columns: new[] { "Type", "OccurredAt" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_PasswordResetTokens_ExpiresAtUtc",
                schema: "iam",
                table: "PasswordResetTokens",
                column: "ExpiresAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_PasswordResetTokens_UsedAtUtc",
                schema: "iam",
                table: "PasswordResetTokens",
                column: "UsedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_PasswordResetTokens_UserId",
                schema: "iam",
                table: "PasswordResetTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PayableSchedules_AccountsPayableId",
                table: "PayableSchedules",
                column: "AccountsPayableId");

            migrationBuilder.CreateIndex(
                name: "IX_PayableSchedules_CompanyId",
                table: "PayableSchedules",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_CompanyId",
                table: "Payments",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_Code",
                schema: "iam",
                table: "Permissions",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_Name",
                schema: "iam",
                table: "Permissions",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PharmacyCriticalAudits_CompanyId_EntityName",
                schema: "pharma",
                table: "PharmacyCriticalAudits",
                columns: new[] { "CompanyId", "EntityName" });

            migrationBuilder.CreateIndex(
                name: "IX_PharmacyCriticalAudits_CompanyId_OccurredAt",
                schema: "pharma",
                table: "PharmacyCriticalAudits",
                columns: new[] { "CompanyId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_PharmacyTraceEvents_CompanyId_BatchNumber",
                schema: "pharma",
                table: "PharmacyTraceEvents",
                columns: new[] { "CompanyId", "BatchNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_PharmacyTraceEvents_CompanyId_PatientReference",
                schema: "pharma",
                table: "PharmacyTraceEvents",
                columns: new[] { "CompanyId", "PatientReference" });

            migrationBuilder.CreateIndex(
                name: "IX_PharmacyTraceEvents_CompanyId_SupplierId",
                schema: "pharma",
                table: "PharmacyTraceEvents",
                columns: new[] { "CompanyId", "SupplierId" });

            migrationBuilder.CreateIndex(
                name: "IX_PharmacyTraceEvents_MovementBatchAllocationId",
                schema: "pharma",
                table: "PharmacyTraceEvents",
                column: "MovementBatchAllocationId");

            migrationBuilder.CreateIndex(
                name: "IX_PharmacyTraceEvents_MovementId",
                schema: "pharma",
                table: "PharmacyTraceEvents",
                column: "MovementId");

            migrationBuilder.CreateIndex(
                name: "IX_PharmacyTraceEvents_StockBatchId",
                schema: "pharma",
                table: "PharmacyTraceEvents",
                column: "StockBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_PharmaProductProfiles_CompanyId_ProductId",
                schema: "pharma",
                table: "PharmaProductProfiles",
                columns: new[] { "CompanyId", "ProductId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PrescriptionItems_CompanyId",
                schema: "pharma",
                table: "PrescriptionItems",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_PrescriptionItems_DispenseLineId",
                schema: "pharma",
                table: "PrescriptionItems",
                column: "DispenseLineId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PrescriptionItems_PrescriptionId",
                schema: "pharma",
                table: "PrescriptionItems",
                column: "PrescriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Prescriptions_CompanyId",
                schema: "pharma",
                table: "Prescriptions",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Prescriptions_DispenseId",
                schema: "pharma",
                table: "Prescriptions",
                column: "DispenseId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PriceListItems_CompanyId_PriceListId_ProductId",
                table: "PriceListItems",
                columns: new[] { "CompanyId", "PriceListId", "ProductId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PriceLists_CompanyId_Name",
                table: "PriceLists",
                columns: new[] { "CompanyId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PricingPolicies_CompanyId",
                table: "PricingPolicies",
                column: "CompanyId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductPharmaInfos_CompanyId_Barcode",
                schema: "pharma",
                table: "ProductPharmaInfos",
                columns: new[] { "CompanyId", "Barcode" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductPharmaInfos_CompanyId_ProductId",
                schema: "pharma",
                table: "ProductPharmaInfos",
                columns: new[] { "CompanyId", "ProductId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_CompanyId_Barcode",
                table: "Products",
                columns: new[] { "CompanyId", "Barcode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_CompanyId_Sku",
                table: "Products",
                columns: new[] { "CompanyId", "Sku" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_UnitOfMeasureId",
                table: "Products",
                column: "UnitOfMeasureId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductUnitConversions_FromUnitOfMeasureId",
                table: "ProductUnitConversions",
                column: "FromUnitOfMeasureId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductUnitConversions_ProductId_FromUnitOfMeasureId_ToUnit~",
                table: "ProductUnitConversions",
                columns: new[] { "ProductId", "FromUnitOfMeasureId", "ToUnitOfMeasureId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductUnitConversions_ToUnitOfMeasureId",
                table: "ProductUnitConversions",
                column: "ToUnitOfMeasureId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseInvoiceIngestionLines_CompanyId",
                schema: "pharma",
                table: "PurchaseInvoiceIngestionLines",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseInvoiceIngestionLines_PurchaseInvoiceIngestionId",
                schema: "pharma",
                table: "PurchaseInvoiceIngestionLines",
                column: "PurchaseInvoiceIngestionId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseInvoiceIngestions_CompanyId_InvoiceNumber",
                schema: "pharma",
                table: "PurchaseInvoiceIngestions",
                columns: new[] { "CompanyId", "InvoiceNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderLines_CompanyId",
                table: "PurchaseOrderLines",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderLines_PurchaseOrderId",
                table: "PurchaseOrderLines",
                column: "PurchaseOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_CompanyId",
                table: "PurchaseOrders",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseSuggestionLines_CompanyId_PurchaseSuggestionId",
                table: "PurchaseSuggestionLines",
                columns: new[] { "CompanyId", "PurchaseSuggestionId" });

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseSuggestionLines_PurchaseSuggestionId",
                table: "PurchaseSuggestionLines",
                column: "PurchaseSuggestionId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseSuggestions_CompanyId_Status_CreatedAt",
                table: "PurchaseSuggestions",
                columns: new[] { "CompanyId", "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_QuarantineHolds_CompanyId",
                schema: "pharma",
                table: "QuarantineHolds",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_QuarantineHolds_StockBatchId",
                schema: "pharma",
                table: "QuarantineHolds",
                column: "StockBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_Recalls_CompanyId",
                schema: "pharma",
                table: "Recalls",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Recalls_StockBatchId",
                schema: "pharma",
                table: "Recalls",
                column: "StockBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_Receipts_CompanyId",
                table: "Receipts",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceivableSchedules_AccountsReceivableId",
                table: "ReceivableSchedules",
                column: "AccountsReceivableId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceivableSchedules_CompanyId",
                table: "ReceivableSchedules",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleAssignments_CompanyPublicId_Status",
                schema: "iam",
                table: "RoleAssignments",
                columns: new[] { "CompanyPublicId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_RoleAssignments_OrganizationId_Status",
                schema: "iam",
                table: "RoleAssignments",
                columns: new[] { "OrganizationId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_RoleAssignments_RoleId",
                schema: "iam",
                table: "RoleAssignments",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleAssignments_UserId_CompanyPublicId_Status",
                schema: "iam",
                table: "RoleAssignments",
                columns: new[] { "UserId", "CompanyPublicId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_RoleAssignments_UserId_Status",
                schema: "iam",
                table: "RoleAssignments",
                columns: new[] { "UserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_PermissionId",
                schema: "iam",
                table: "RolePermissions",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_Name",
                schema: "iam",
                table: "Roles",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_SalesDocumentLines_CompanyId",
                table: "SalesDocumentLines",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesDocumentLines_SalesDocumentId",
                table: "SalesDocumentLines",
                column: "SalesDocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesDocuments_CompanyId",
                table: "SalesDocuments",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesQuoteLines_CompanyId",
                schema: "sales",
                table: "SalesQuoteLines",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesQuoteLines_QuoteId",
                schema: "sales",
                table: "SalesQuoteLines",
                column: "QuoteId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesQuotes_CompanyId_PublicId",
                schema: "sales",
                table: "SalesQuotes",
                columns: new[] { "CompanyId", "PublicId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalesQuotes_CompanyId_Status",
                schema: "sales",
                table: "SalesQuotes",
                columns: new[] { "CompanyId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_StockBatches_CompanyId_ExpiryDate",
                schema: "pharma",
                table: "StockBatches",
                columns: new[] { "CompanyId", "ExpiryDate" });

            migrationBuilder.CreateIndex(
                name: "IX_StockBatches_CompanyId_ProductId_WarehouseId_BatchNumber_Ex~",
                schema: "pharma",
                table: "StockBatches",
                columns: new[] { "CompanyId", "ProductId", "WarehouseId", "BatchNumber", "ExpiryDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StockBatches_WarehouseLocationId",
                schema: "pharma",
                table: "StockBatches",
                column: "WarehouseLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_StockLedgerEntries_CompanyId_ProductId_WarehouseId",
                schema: "pharma",
                table: "StockLedgerEntries",
                columns: new[] { "CompanyId", "ProductId", "WarehouseId" });

            migrationBuilder.CreateIndex(
                name: "IX_StockLedgerEntries_CompanyId_StockBatchId",
                schema: "pharma",
                table: "StockLedgerEntries",
                columns: new[] { "CompanyId", "StockBatchId" });

            migrationBuilder.CreateIndex(
                name: "IX_StockoutThresholds_CompanyId_ProductId_WarehouseId",
                schema: "pharma",
                table: "StockoutThresholds",
                columns: new[] { "CompanyId", "ProductId", "WarehouseId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Stocks_CompanyId_ProductId_WarehouseId",
                table: "Stocks",
                columns: new[] { "CompanyId", "ProductId", "WarehouseId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Subdivision_CountryId_Code",
                schema: "masterdata",
                table: "Subdivision",
                columns: new[] { "CountryId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Subdivision_CountryId_Level",
                schema: "masterdata",
                table: "Subdivision",
                columns: new[] { "CountryId", "Level" });

            migrationBuilder.CreateIndex(
                name: "IX_Subdivision_Name",
                schema: "masterdata",
                table: "Subdivision",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Subdivision_ParentSubdivisionId",
                schema: "masterdata",
                table: "Subdivision",
                column: "ParentSubdivisionId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionBillingCycles_SubscriptionId",
                table: "SubscriptionBillingCycles",
                column: "SubscriptionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionBillingCycles_SubscriptionId_StartsAt",
                table: "SubscriptionBillingCycles",
                columns: new[] { "SubscriptionId", "StartsAt" });

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPlanFeatures_PlanId_FeatureKey",
                table: "SubscriptionPlanFeatures",
                columns: new[] { "PlanId", "FeatureKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPlans_Code",
                table: "SubscriptionPlans",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_CompanyId_Status",
                table: "Subscriptions",
                columns: new[] { "CompanyId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_PlanId",
                table: "Subscriptions",
                column: "PlanId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionSeatCounts_SubscriptionId",
                table: "SubscriptionSeatCounts",
                column: "SubscriptionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionTrials_SubscriptionId",
                table: "SubscriptionTrials",
                column: "SubscriptionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionUsage_SubscriptionId_RecordedAt",
                table: "SubscriptionUsage",
                columns: new[] { "SubscriptionId", "RecordedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_CompanyId_Name",
                table: "Suppliers",
                columns: new[] { "CompanyId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_CompanyId_TaxId",
                table: "Suppliers",
                columns: new[] { "CompanyId", "TaxId" });

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_CountryId",
                table: "Suppliers",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_DefaultCurrencyId",
                table: "Suppliers",
                column: "DefaultCurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_CompanyPublicId_Epc",
                schema: "rfid",
                table: "Tags",
                columns: new[] { "CompanyPublicId", "Epc" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TaxBookEntries_CompanyId_Year_Month_BookType",
                table: "TaxBookEntries",
                columns: new[] { "CompanyId", "Year", "Month", "BookType" });

            migrationBuilder.CreateIndex(
                name: "IX_TaxDeclarations_CompanyId_Year_Month_DeclarationType",
                table: "TaxDeclarations",
                columns: new[] { "CompanyId", "Year", "Month", "DeclarationType" });

            migrationBuilder.CreateIndex(
                name: "IX_TaxEntities_CompanyPublicId",
                schema: "iam",
                table: "TaxEntities",
                column: "CompanyPublicId");

            migrationBuilder.CreateIndex(
                name: "IX_TaxEntities_TaxId",
                schema: "iam",
                table: "TaxEntities",
                column: "TaxId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Taxes_CompanyId_Code",
                table: "Taxes",
                columns: new[] { "CompanyId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TaxGroups_CompanyId",
                table: "TaxGroups",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_TaxRules_CompanyId",
                table: "TaxRules",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_TaxRules_TaxGroupId",
                table: "TaxRules",
                column: "TaxGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_TaxRules_TaxId",
                table: "TaxRules",
                column: "TaxId");

            migrationBuilder.CreateIndex(
                name: "IX_UnitOfMeasureExternalMappings_Scheme_Code",
                schema: "masterdata",
                table: "UnitOfMeasureExternalMappings",
                columns: new[] { "Scheme", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UnitOfMeasureExternalMappings_UnitOfMeasureId",
                schema: "masterdata",
                table: "UnitOfMeasureExternalMappings",
                column: "UnitOfMeasureId");

            migrationBuilder.CreateIndex(
                name: "IX_UnitOfMeasures_CanonicalCode",
                schema: "masterdata",
                table: "UnitOfMeasures",
                column: "CanonicalCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UnitOfMeasureTranslations_UnitOfMeasureId_Culture",
                schema: "masterdata",
                table: "UnitOfMeasureTranslations",
                columns: new[] { "UnitOfMeasureId", "Culture" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserAuditLog_OccurredAt",
                schema: "iam",
                table: "UserAuditLog",
                column: "OccurredAt");

            migrationBuilder.CreateIndex(
                name: "IX_UserPasswordHistory_UserId_CreatedAtUtc",
                schema: "iam",
                table: "UserPasswordHistory",
                columns: new[] { "UserId", "CreatedAtUtc" },
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_UserId",
                schema: "iam",
                table: "UserProfiles",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                schema: "iam",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserSessions_RefreshTokenHash",
                schema: "iam",
                table: "UserSessions",
                column: "RefreshTokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserSessions_SessionId",
                schema: "iam",
                table: "UserSessions",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_UserSessions_UserId",
                schema: "iam",
                table: "UserSessions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseLocations_CompanyId_WarehouseId_Aisle_Rack_Side_Le~",
                schema: "wms",
                table: "WarehouseLocations",
                columns: new[] { "CompanyId", "WarehouseId", "Aisle", "Rack", "Side", "Level", "Slot" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseLocations_WarehouseId",
                schema: "wms",
                table: "WarehouseLocations",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseOperations_CompanyId_WarehouseId_ReferenceType_Ref~",
                schema: "wms",
                table: "WarehouseOperations",
                columns: new[] { "CompanyId", "WarehouseId", "ReferenceType", "ReferenceId" });

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseOperations_WarehouseId",
                schema: "wms",
                table: "WarehouseOperations",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_Warehouses_CompanyId_Code",
                table: "Warehouses",
                columns: new[] { "CompanyId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Wastes_CompanyId_CreatedAt",
                schema: "pharma",
                table: "Wastes",
                columns: new[] { "CompanyId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Wastes_StockBatchId",
                schema: "pharma",
                table: "Wastes",
                column: "StockBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowDefinitions_DocumentType_IsActive",
                table: "WorkflowDefinitions",
                columns: new[] { "DocumentType", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowHistories_WorkflowInstanceId",
                table: "WorkflowHistories",
                column: "WorkflowInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowInstances_DocumentType_DocumentId",
                table: "WorkflowInstances",
                columns: new[] { "DocumentType", "DocumentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowStates_WorkflowDefinitionId_Name",
                table: "WorkflowStates",
                columns: new[] { "WorkflowDefinitionId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowTasks_WorkflowInstanceId_Status",
                table: "WorkflowTasks",
                columns: new[] { "WorkflowInstanceId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowTransitions_FromStateId",
                table: "WorkflowTransitions",
                column: "FromStateId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowTransitions_ToStateId",
                table: "WorkflowTransitions",
                column: "ToStateId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowTransitions_WorkflowDefinitionId_FromStateId",
                table: "WorkflowTransitions",
                columns: new[] { "WorkflowDefinitionId", "FromStateId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccessSessions",
                schema: "rfid");

            migrationBuilder.DropTable(
                name: "AccountingAutomationRules");

            migrationBuilder.DropTable(
                name: "AccountingPeriods");

            migrationBuilder.DropTable(
                name: "AccountsPayable");

            migrationBuilder.DropTable(
                name: "AccountsReceivable");

            migrationBuilder.DropTable(
                name: "Address",
                schema: "masterdata");

            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "BankAccounts");

            migrationBuilder.DropTable(
                name: "BankTransactions");

            migrationBuilder.DropTable(
                name: "BlockHolds",
                schema: "pharma");

            migrationBuilder.DropTable(
                name: "ChartOfAccountsTemplates",
                schema: "masterdata");

            migrationBuilder.DropTable(
                name: "CompanyAccountingSettings");

            migrationBuilder.DropTable(
                name: "CompanyChartOfAccounts");

            migrationBuilder.DropTable(
                name: "CompanyCurrencies",
                schema: "public");

            migrationBuilder.DropTable(
                name: "CompanyFeatureAudits",
                schema: "public");

            migrationBuilder.DropTable(
                name: "CompanyFeatures",
                schema: "public");

            migrationBuilder.DropTable(
                name: "CompanyLinks",
                schema: "iam");

            migrationBuilder.DropTable(
                name: "CompanyUnitsOfMeasure");

            migrationBuilder.DropTable(
                name: "CompanyUsers",
                schema: "iam");

            migrationBuilder.DropTable(
                name: "ControlledSubstanceLedgerAudits",
                schema: "pharma");

            migrationBuilder.DropTable(
                name: "ControlledSubstanceLedgers",
                schema: "pharma");

            migrationBuilder.DropTable(
                name: "ControlledSubstanceReportExports",
                schema: "pharma");

            migrationBuilder.DropTable(
                name: "CreditNotes");

            migrationBuilder.DropTable(
                name: "Customers");

            migrationBuilder.DropTable(
                name: "DebitNotes");

            migrationBuilder.DropTable(
                name: "DeliveryConfirmations",
                schema: "pharma");

            migrationBuilder.DropTable(
                name: "DepreciationScheduleLines");

            migrationBuilder.DropTable(
                name: "DispatchConfirmations",
                schema: "wms");

            migrationBuilder.DropTable(
                name: "DocumentLinks",
                schema: "documents");

            migrationBuilder.DropTable(
                name: "DocumentSearchEntries");

            migrationBuilder.DropTable(
                name: "DocumentVersions",
                schema: "documents");

            migrationBuilder.DropTable(
                name: "DteDocuments");

            migrationBuilder.DropTable(
                name: "EventInbox",
                schema: "rfid");

            migrationBuilder.DropTable(
                name: "ExpirationAlertRules",
                schema: "pharma");

            migrationBuilder.DropTable(
                name: "FeatureCatalog",
                schema: "masterdata");

            migrationBuilder.DropTable(
                name: "GoodsReceiptLines");

            migrationBuilder.DropTable(
                name: "InvoiceLines");

            migrationBuilder.DropTable(
                name: "JournalEntryLines");

            migrationBuilder.DropTable(
                name: "Journals");

            migrationBuilder.DropTable(
                name: "MovementLinks",
                schema: "rfid");

            migrationBuilder.DropTable(
                name: "OperatorCredentials",
                schema: "rfid");

            migrationBuilder.DropTable(
                name: "OrganizationMembers",
                schema: "iam");

            migrationBuilder.DropTable(
                name: "OutboxMessages");

            migrationBuilder.DropTable(
                name: "PasswordResetTokens",
                schema: "iam");

            migrationBuilder.DropTable(
                name: "PayableSchedules");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "PharmacyCriticalAudits",
                schema: "pharma");

            migrationBuilder.DropTable(
                name: "PharmacyTraceEvents",
                schema: "pharma");

            migrationBuilder.DropTable(
                name: "PharmaProductProfiles",
                schema: "pharma");

            migrationBuilder.DropTable(
                name: "PrescriptionItems",
                schema: "pharma");

            migrationBuilder.DropTable(
                name: "PriceListItems");

            migrationBuilder.DropTable(
                name: "PriceLists");

            migrationBuilder.DropTable(
                name: "PricingPolicies");

            migrationBuilder.DropTable(
                name: "ProductPharmaInfos",
                schema: "pharma");

            migrationBuilder.DropTable(
                name: "ProductUnitConversions");

            migrationBuilder.DropTable(
                name: "PurchaseInvoiceIngestionLines",
                schema: "pharma");

            migrationBuilder.DropTable(
                name: "PurchaseOrderLines");

            migrationBuilder.DropTable(
                name: "PurchaseSuggestionLines");

            migrationBuilder.DropTable(
                name: "QuarantineHolds",
                schema: "pharma");

            migrationBuilder.DropTable(
                name: "Recalls",
                schema: "pharma");

            migrationBuilder.DropTable(
                name: "Receipts");

            migrationBuilder.DropTable(
                name: "ReceivableSchedules");

            migrationBuilder.DropTable(
                name: "RoleAssignments",
                schema: "iam");

            migrationBuilder.DropTable(
                name: "RolePermissions",
                schema: "iam");

            migrationBuilder.DropTable(
                name: "SalesDocumentLines");

            migrationBuilder.DropTable(
                name: "SalesQuoteLines",
                schema: "sales");

            migrationBuilder.DropTable(
                name: "StockLedgerEntries",
                schema: "pharma");

            migrationBuilder.DropTable(
                name: "StockoutThresholds",
                schema: "pharma");

            migrationBuilder.DropTable(
                name: "Stocks");

            migrationBuilder.DropTable(
                name: "SubscriptionBillingCycles");

            migrationBuilder.DropTable(
                name: "SubscriptionPlanFeatures");

            migrationBuilder.DropTable(
                name: "SubscriptionSeatCounts");

            migrationBuilder.DropTable(
                name: "SubscriptionTrials");

            migrationBuilder.DropTable(
                name: "SubscriptionUsage");

            migrationBuilder.DropTable(
                name: "Suppliers");

            migrationBuilder.DropTable(
                name: "Tags",
                schema: "rfid");

            migrationBuilder.DropTable(
                name: "TaxBookEntries");

            migrationBuilder.DropTable(
                name: "TaxDeclarations");

            migrationBuilder.DropTable(
                name: "TaxEntities",
                schema: "iam");

            migrationBuilder.DropTable(
                name: "TaxRules");

            migrationBuilder.DropTable(
                name: "UnitOfMeasureExternalMappings",
                schema: "masterdata");

            migrationBuilder.DropTable(
                name: "UnitOfMeasureTranslations",
                schema: "masterdata");

            migrationBuilder.DropTable(
                name: "UserAuditLog",
                schema: "iam");

            migrationBuilder.DropTable(
                name: "UserPasswordHistory",
                schema: "iam");

            migrationBuilder.DropTable(
                name: "UserProfiles",
                schema: "iam");

            migrationBuilder.DropTable(
                name: "UserSessions",
                schema: "iam");

            migrationBuilder.DropTable(
                name: "WarehouseOperations",
                schema: "wms");

            migrationBuilder.DropTable(
                name: "Wastes",
                schema: "pharma");

            migrationBuilder.DropTable(
                name: "WorkflowHistories");

            migrationBuilder.DropTable(
                name: "WorkflowInstances");

            migrationBuilder.DropTable(
                name: "WorkflowTasks");

            migrationBuilder.DropTable(
                name: "WorkflowTransitions");

            migrationBuilder.DropTable(
                name: "Locality",
                schema: "masterdata");

            migrationBuilder.DropTable(
                name: "BankStatements");

            migrationBuilder.DropTable(
                name: "OfficialControlledBookEntries",
                schema: "pharma");

            migrationBuilder.DropTable(
                name: "DepreciationSchedules");

            migrationBuilder.DropTable(
                name: "Documents",
                schema: "documents");

            migrationBuilder.DropTable(
                name: "GoodsReceipts");

            migrationBuilder.DropTable(
                name: "Invoices");

            migrationBuilder.DropTable(
                name: "JournalEntries");

            migrationBuilder.DropTable(
                name: "Operators",
                schema: "rfid");

            migrationBuilder.DropTable(
                name: "AccountsPayables");

            migrationBuilder.DropTable(
                name: "MovementBatchAllocations",
                schema: "pharma");

            migrationBuilder.DropTable(
                name: "DispenseLines",
                schema: "pharma");

            migrationBuilder.DropTable(
                name: "Prescriptions",
                schema: "pharma");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "PurchaseInvoiceIngestions",
                schema: "pharma");

            migrationBuilder.DropTable(
                name: "PurchaseOrders");

            migrationBuilder.DropTable(
                name: "PurchaseSuggestions");

            migrationBuilder.DropTable(
                name: "AccountsReceivables");

            migrationBuilder.DropTable(
                name: "Permissions",
                schema: "iam");

            migrationBuilder.DropTable(
                name: "Roles",
                schema: "iam");

            migrationBuilder.DropTable(
                name: "SalesDocuments");

            migrationBuilder.DropTable(
                name: "SalesQuotes",
                schema: "sales");

            migrationBuilder.DropTable(
                name: "Subscriptions");

            migrationBuilder.DropTable(
                name: "Currency",
                schema: "masterdata");

            migrationBuilder.DropTable(
                name: "TaxGroups");

            migrationBuilder.DropTable(
                name: "Taxes");

            migrationBuilder.DropTable(
                name: "Users",
                schema: "iam");

            migrationBuilder.DropTable(
                name: "WorkflowStates");

            migrationBuilder.DropTable(
                name: "City",
                schema: "masterdata");

            migrationBuilder.DropTable(
                name: "FixedAssets");

            migrationBuilder.DropTable(
                name: "InventoryMovements");

            migrationBuilder.DropTable(
                name: "StockBatches",
                schema: "pharma");

            migrationBuilder.DropTable(
                name: "Dispenses",
                schema: "pharma");

            migrationBuilder.DropTable(
                name: "UnitOfMeasures",
                schema: "masterdata");

            migrationBuilder.DropTable(
                name: "SubscriptionPlans");

            migrationBuilder.DropTable(
                name: "WorkflowDefinitions");

            migrationBuilder.DropTable(
                name: "Subdivision",
                schema: "masterdata");

            migrationBuilder.DropTable(
                name: "AssetCategories");

            migrationBuilder.DropTable(
                name: "WarehouseLocations",
                schema: "wms");

            migrationBuilder.DropTable(
                name: "Country",
                schema: "masterdata");

            migrationBuilder.DropTable(
                name: "Warehouses");

            migrationBuilder.DropTable(
                name: "Companies");

            migrationBuilder.DropTable(
                name: "Organizations",
                schema: "iam");
        }
    }
}
