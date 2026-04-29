using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ERP.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddWsuFifoValuation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "QuantityAvailable",
                schema: "wsu",
                table: "OrderItems",
                type: "numeric(18,3)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "OrderItemConsumptions",
                schema: "wsu",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OutOrderItemId = table.Column<long>(type: "bigint", nullable: false),
                    InOrderItemId = table.Column<long>(type: "bigint", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,3)", nullable: false),
                    UnitCost = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TotalCost = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItemConsumptions", x => x.Id);
                    table.CheckConstraint("CK_OrderItemConsumptions_Quantity", "\"Quantity\" > 0");
                    table.CheckConstraint("CK_OrderItemConsumptions_TotalCost", "\"TotalCost\" >= 0");
                    table.CheckConstraint("CK_OrderItemConsumptions_UnitCost", "\"UnitCost\" >= 0");
                    table.ForeignKey(
                        name: "FK_OrderItemConsumptions_OrderItems_InOrderItemId",
                        column: x => x.InOrderItemId,
                        principalSchema: "wsu",
                        principalTable: "OrderItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderItemConsumptions_OrderItems_OutOrderItemId",
                        column: x => x.OutOrderItemId,
                        principalSchema: "wsu",
                        principalTable: "OrderItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_ProductPublicId_QuantityAvailable",
                schema: "wsu",
                table: "OrderItems",
                columns: new[] { "ProductPublicId", "QuantityAvailable" });

            migrationBuilder.Sql(
                """
                UPDATE wsu."OrderItems" oi
                SET "QuantityAvailable" = CASE
                    WHEN o."OrderType" IN ('Inbound', 'Replenishment') THEN oi."Quantity"
                    ELSE 0
                END
                FROM wsu."Orders" o
                WHERE o."Id" = oi."OrderId";
                """);

            migrationBuilder.AddCheckConstraint(
                name: "CK_OrderItems_QuantityAvailable",
                schema: "wsu",
                table: "OrderItems",
                sql: "\"QuantityAvailable\" >= 0 AND \"QuantityAvailable\" <= \"Quantity\"");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItemConsumptions_InOrderItemId",
                schema: "wsu",
                table: "OrderItemConsumptions",
                column: "InOrderItemId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItemConsumptions_OutOrderItemId",
                schema: "wsu",
                table: "OrderItemConsumptions",
                column: "OutOrderItemId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItemConsumptions_OutOrderItemId_InOrderItemId",
                schema: "wsu",
                table: "OrderItemConsumptions",
                columns: new[] { "OutOrderItemId", "InOrderItemId" });

            migrationBuilder.CreateIndex(
                name: "IX_OrderItemConsumptions_PublicId",
                schema: "wsu",
                table: "OrderItemConsumptions",
                column: "PublicId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderItemConsumptions",
                schema: "wsu");

            migrationBuilder.DropIndex(
                name: "IX_OrderItems_ProductPublicId_QuantityAvailable",
                schema: "wsu",
                table: "OrderItems");

            migrationBuilder.DropCheckConstraint(
                name: "CK_OrderItems_QuantityAvailable",
                schema: "wsu",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "QuantityAvailable",
                schema: "wsu",
                table: "OrderItems");
        }
    }
}
