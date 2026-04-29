using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveOrderItemWarehouseSnapshotColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CodigoBodega",
                schema: "wsu",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "UbicacionBodega",
                schema: "wsu",
                table: "OrderItems");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CodigoBodega",
                schema: "wsu",
                table: "OrderItems",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UbicacionBodega",
                schema: "wsu",
                table: "OrderItems",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);
        }
    }
}
