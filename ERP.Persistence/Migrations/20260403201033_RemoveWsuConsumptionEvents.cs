using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ERP.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveWsuConsumptionEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConsumptionEvents",
                schema: "wsu");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConsumptionEvents",
                schema: "wsu",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ClientProductWarehouseId = table.Column<long>(type: "bigint", nullable: false),
                    CompanyPublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConsumedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ConsumedByUserPublicId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserPublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    SourceEventPublicId = table.Column<Guid>(type: "uuid", nullable: true),
                    SourceType = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsumptionEvents", x => x.Id);
                    table.CheckConstraint("CK_ConsumptionEvents_Quantity", "\"Quantity\" > 0");
                    table.CheckConstraint("CK_ConsumptionEvents_TotalAmount", "\"TotalAmount\" >= 0");
                    table.CheckConstraint("CK_ConsumptionEvents_UnitPrice", "\"UnitPrice\" >= 0");
                    table.ForeignKey(
                        name: "FK_ConsumptionEvents_ClientProductWarehouses_ClientProductWare~",
                        column: x => x.ClientProductWarehouseId,
                        principalSchema: "wsu",
                        principalTable: "ClientProductWarehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

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
    }
}
