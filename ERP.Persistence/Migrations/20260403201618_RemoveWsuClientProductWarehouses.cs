using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ERP.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveWsuClientProductWarehouses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClientProductWarehouses",
                schema: "wsu");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ClientProductWarehouses",
                schema: "wsu",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyPublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CustomerProductName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    CustomerSku = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsStackable = table.Column<bool>(type: "boolean", nullable: true),
                    LastUnitSalePriceUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LengthCm = table.Column<decimal>(type: "numeric(18,3)", nullable: true),
                    MinimumSaleLot = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ProductPublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    StorageType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    TargetCoverageMonths = table.Column<int>(type: "integer", nullable: false, defaultValue: 3),
                    UnitSalePrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    WarehousePublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    WeightKg = table.Column<decimal>(type: "numeric(18,3)", nullable: true),
                    WidthCm = table.Column<decimal>(type: "numeric(18,3)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientProductWarehouses", x => x.Id);
                    table.CheckConstraint("CK_ClientProductWarehouses_LengthCm", "\"LengthCm\" IS NULL OR \"LengthCm\" >= 0");
                    table.CheckConstraint("CK_ClientProductWarehouses_MinimumSaleLot", "\"MinimumSaleLot\" > 0");
                    table.CheckConstraint("CK_ClientProductWarehouses_TargetCoverageMonths", "\"TargetCoverageMonths\" > 0");
                    table.CheckConstraint("CK_ClientProductWarehouses_UnitSalePrice", "\"UnitSalePrice\" >= 0");
                    table.CheckConstraint("CK_ClientProductWarehouses_WeightKg", "\"WeightKg\" IS NULL OR \"WeightKg\" >= 0");
                    table.CheckConstraint("CK_ClientProductWarehouses_WidthCm", "\"WidthCm\" IS NULL OR \"WidthCm\" >= 0");
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
        }
    }
}
