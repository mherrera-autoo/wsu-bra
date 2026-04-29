using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ERP.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class WsuRfidReconciliationAndOrderItemSimplificationV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OrderItems_OrderId_TipoOrden",
                schema: "wsu",
                table: "OrderItems");

            migrationBuilder.DropCheckConstraint(
                name: "CK_OrderItems_Quantity",
                schema: "wsu",
                table: "OrderItems");

            migrationBuilder.DropCheckConstraint(
                name: "CK_OrderItems_QuantityAvailable",
                schema: "wsu",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "LineTotal",
                schema: "wsu",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "Quantity",
                schema: "wsu",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "TipoOrden",
                schema: "wsu",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "UnitPrice",
                schema: "wsu",
                table: "OrderItems");

            migrationBuilder.CreateTable(
                name: "OrderItemReconciliations",
                schema: "wsu",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrderId = table.Column<long>(type: "bigint", nullable: false),
                    OrderItemId = table.Column<long>(type: "bigint", nullable: false),
                    ReconciledQuantity = table.Column<decimal>(type: "numeric(18,3)", nullable: false),
                    ReconciledAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ReconciledByUserPublicId = table.Column<Guid>(type: "uuid", nullable: true),
                    SourceType = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItemReconciliations", x => x.Id);
                    table.CheckConstraint("CK_OrderItemReconciliations_ReconciledQuantity", "\"ReconciledQuantity\" > 0");
                    table.ForeignKey(
                        name: "FK_OrderItemReconciliations_OrderItems_OrderItemId",
                        column: x => x.OrderItemId,
                        principalSchema: "wsu",
                        principalTable: "OrderItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.AddCheckConstraint(
                name: "CK_OrderItems_QuantityAvailable",
                schema: "wsu",
                table: "OrderItems",
                sql: "\"QuantityAvailable\" >= 0 AND \"QuantityAvailable\" <= abs(\"VariacionStock\")");

            migrationBuilder.AddCheckConstraint(
                name: "CK_OrderItems_VariacionStock",
                schema: "wsu",
                table: "OrderItems",
                sql: "\"VariacionStock\" <> 0");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItemReconciliations_OrderId",
                schema: "wsu",
                table: "OrderItemReconciliations",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItemReconciliations_OrderItemId",
                schema: "wsu",
                table: "OrderItemReconciliations",
                column: "OrderItemId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItemReconciliations_PublicId",
                schema: "wsu",
                table: "OrderItemReconciliations",
                column: "PublicId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderItemReconciliations",
                schema: "wsu");

            migrationBuilder.DropCheckConstraint(
                name: "CK_OrderItems_QuantityAvailable",
                schema: "wsu",
                table: "OrderItems");

            migrationBuilder.DropCheckConstraint(
                name: "CK_OrderItems_VariacionStock",
                schema: "wsu",
                table: "OrderItems");

            migrationBuilder.AddColumn<decimal>(
                name: "LineTotal",
                schema: "wsu",
                table: "OrderItems",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Quantity",
                schema: "wsu",
                table: "OrderItems",
                type: "numeric(18,3)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "TipoOrden",
                schema: "wsu",
                table: "OrderItems",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "UnitPrice",
                schema: "wsu",
                table: "OrderItems",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_OrderId_TipoOrden",
                schema: "wsu",
                table: "OrderItems",
                columns: new[] { "OrderId", "TipoOrden" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_OrderItems_Quantity",
                schema: "wsu",
                table: "OrderItems",
                sql: "\"Quantity\" > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_OrderItems_QuantityAvailable",
                schema: "wsu",
                table: "OrderItems",
                sql: "\"QuantityAvailable\" >= 0 AND \"QuantityAvailable\" <= \"Quantity\"");
        }
    }
}
