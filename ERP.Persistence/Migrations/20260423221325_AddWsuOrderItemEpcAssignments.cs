using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ERP.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddWsuOrderItemEpcAssignments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OrderItemEpcAssignments",
                schema: "wsu",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrderId = table.Column<long>(type: "bigint", nullable: false),
                    OrderItemId = table.Column<long>(type: "bigint", nullable: false),
                    Epc = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    IsChecked = table.Column<bool>(type: "boolean", nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItemEpcAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderItemEpcAssignments_OrderItems_OrderItemId",
                        column: x => x.OrderItemId,
                        principalSchema: "wsu",
                        principalTable: "OrderItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderItemEpcAssignments_OrderId",
                schema: "wsu",
                table: "OrderItemEpcAssignments",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItemEpcAssignments_OrderId_Epc",
                schema: "wsu",
                table: "OrderItemEpcAssignments",
                columns: new[] { "OrderId", "Epc" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderItemEpcAssignments_OrderItemId",
                schema: "wsu",
                table: "OrderItemEpcAssignments",
                column: "OrderItemId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItemEpcAssignments_PublicId",
                schema: "wsu",
                table: "OrderItemEpcAssignments",
                column: "PublicId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderItemEpcAssignments",
                schema: "wsu");
        }
    }
}
