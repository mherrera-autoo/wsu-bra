using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ERP.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddWsuOrderMovementOperators : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OrderMovementOperators",
                schema: "wsu",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrderId = table.Column<long>(type: "bigint", nullable: false),
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
                    table.PrimaryKey("PK_OrderMovementOperators", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderMovementOperators_Orders_OrderId",
                        column: x => x.OrderId,
                        principalSchema: "wsu",
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderMovementOperators_OrderId",
                schema: "wsu",
                table: "OrderMovementOperators",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderMovementOperators_OrderId_CodigoOperador",
                schema: "wsu",
                table: "OrderMovementOperators",
                columns: new[] { "OrderId", "CodigoOperador" });

            migrationBuilder.CreateIndex(
                name: "IX_OrderMovementOperators_PublicId",
                schema: "wsu",
                table: "OrderMovementOperators",
                column: "PublicId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderMovementOperators",
                schema: "wsu");
        }
    }
}
