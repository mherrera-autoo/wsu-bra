using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ERP.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddWsuModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "wsu");

            migrationBuilder.CreateTable(
                name: "ClientProductWarehouses",
                schema: "wsu",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyPublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    WarehousePublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductPublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerSku = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    CustomerProductName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    PlannedInitialStock = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TargetCoverageMonths = table.Column<int>(type: "integer", nullable: false, defaultValue: 3),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientProductWarehouses", x => x.Id);
                    table.CheckConstraint("CK_ClientProductWarehouses_PlannedInitialStock", "\"PlannedInitialStock\" >= 0");
                    table.CheckConstraint("CK_ClientProductWarehouses_TargetCoverageMonths", "\"TargetCoverageMonths\" > 0");
                });

            migrationBuilder.CreateTable(
                name: "ConsumptionEvents",
                schema: "wsu",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyPublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClientProductWarehouseId = table.Column<long>(type: "bigint", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ConsumedByUserPublicId = table.Column<Guid>(type: "uuid", nullable: true),
                    SourceType = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    SourceEventPublicId = table.Column<Guid>(type: "uuid", nullable: true),
                    ConsumedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsumptionEvents", x => x.Id);
                    table.CheckConstraint("CK_ConsumptionEvents_Quantity", "\"Quantity\" > 0");
                    table.ForeignKey(
                        name: "FK_ConsumptionEvents_ClientProductWarehouses_ClientProductWare~",
                        column: x => x.ClientProductWarehouseId,
                        principalSchema: "wsu",
                        principalTable: "ClientProductWarehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClientProductWarehouses_CompanyPublicId_ProductPublicId_IsA~",
                schema: "wsu",
                table: "ClientProductWarehouses",
                columns: new[] { "CompanyPublicId", "ProductPublicId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_ClientProductWarehouses_CompanyPublicId_WarehousePublicId_I~",
                schema: "wsu",
                table: "ClientProductWarehouses",
                columns: new[] { "CompanyPublicId", "WarehousePublicId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_ClientProductWarehouses_CompanyPublicId_WarehousePublicId_P~",
                schema: "wsu",
                table: "ClientProductWarehouses",
                columns: new[] { "CompanyPublicId", "WarehousePublicId", "ProductPublicId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClientProductWarehouses_PublicId",
                schema: "wsu",
                table: "ClientProductWarehouses",
                column: "PublicId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConsumptionEvents_ClientProductWarehouseId",
                schema: "wsu",
                table: "ConsumptionEvents",
                column: "ClientProductWarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsumptionEvents_CompanyPublicId_ClientProductWarehouseId_~",
                schema: "wsu",
                table: "ConsumptionEvents",
                columns: new[] { "CompanyPublicId", "ClientProductWarehouseId", "ConsumedAt" },
                descending: new[] { false, false, true });

            migrationBuilder.CreateIndex(
                name: "IX_ConsumptionEvents_CompanyPublicId_SourceType_SourceEventPub~",
                schema: "wsu",
                table: "ConsumptionEvents",
                columns: new[] { "CompanyPublicId", "SourceType", "SourceEventPublicId" },
                unique: true,
                filter: "\"SourceEventPublicId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ConsumptionEvents_PublicId",
                schema: "wsu",
                table: "ConsumptionEvents",
                column: "PublicId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConsumptionEvents",
                schema: "wsu");

            migrationBuilder.DropTable(
                name: "ClientProductWarehouses",
                schema: "wsu");
        }
    }
}
