using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ERP.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MoveWsuOrderItemOperatorFieldsToInventoryMovementOperators : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CodigoOperador",
                schema: "wsu",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "FechaMovimiento",
                schema: "wsu",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "HoraFinMovimiento",
                schema: "wsu",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "HoraInicioMovimiento",
                schema: "wsu",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "NombreOperador",
                schema: "wsu",
                table: "OrderItems");

            migrationBuilder.CreateTable(
                name: "InventoryMovementOperators",
                schema: "wsu",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    InventoryMovementId = table.Column<long>(type: "bigint", nullable: false),
                    CodigoOperador = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    NombreOperador = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    FechaMovimiento = table.Column<DateOnly>(type: "date", nullable: true),
                    HoraInicioMovimiento = table.Column<TimeOnly>(type: "time", nullable: true),
                    HoraFinMovimiento = table.Column<TimeOnly>(type: "time", nullable: true),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryMovementOperators", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventoryMovementOperators_InventoryMovements_InventoryMove~",
                        column: x => x.InventoryMovementId,
                        principalSchema: "wsu",
                        principalTable: "InventoryMovements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InventoryMovementOperators_InventoryMovementId",
                schema: "wsu",
                table: "InventoryMovementOperators",
                column: "InventoryMovementId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryMovementOperators_InventoryMovementId_CodigoOperad~",
                schema: "wsu",
                table: "InventoryMovementOperators",
                columns: new[] { "InventoryMovementId", "CodigoOperador" });

            migrationBuilder.CreateIndex(
                name: "IX_InventoryMovementOperators_PublicId",
                schema: "wsu",
                table: "InventoryMovementOperators",
                column: "PublicId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InventoryMovementOperators",
                schema: "wsu");

            migrationBuilder.AddColumn<string>(
                name: "CodigoOperador",
                schema: "wsu",
                table: "OrderItems",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "FechaMovimiento",
                schema: "wsu",
                table: "OrderItems",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "HoraFinMovimiento",
                schema: "wsu",
                table: "OrderItems",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "HoraInicioMovimiento",
                schema: "wsu",
                table: "OrderItems",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NombreOperador",
                schema: "wsu",
                table: "OrderItems",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);
        }
    }
}
