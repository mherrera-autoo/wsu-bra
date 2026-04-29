using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRfidTagWarehouseScopeAndDropProductPublicId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tags_CompanyPublicId_Epc",
                schema: "rfid",
                table: "Tags");

            migrationBuilder.DropColumn(
                name: "ProductPublicId",
                schema: "rfid",
                table: "Tags");

            migrationBuilder.AddColumn<Guid>(
                name: "WarehousePublicId",
                schema: "rfid",
                table: "Tags",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tags_CompanyPublicId_Epc",
                schema: "rfid",
                table: "Tags",
                columns: new[] { "CompanyPublicId", "Epc" },
                unique: true,
                filter: "\"WarehousePublicId\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_CompanyPublicId_WarehousePublicId_Epc",
                schema: "rfid",
                table: "Tags",
                columns: new[] { "CompanyPublicId", "WarehousePublicId", "Epc" },
                unique: true,
                filter: "\"WarehousePublicId\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tags_CompanyPublicId_Epc",
                schema: "rfid",
                table: "Tags");

            migrationBuilder.DropIndex(
                name: "IX_Tags_CompanyPublicId_WarehousePublicId_Epc",
                schema: "rfid",
                table: "Tags");

            migrationBuilder.DropColumn(
                name: "WarehousePublicId",
                schema: "rfid",
                table: "Tags");

            migrationBuilder.AddColumn<Guid>(
                name: "ProductPublicId",
                schema: "rfid",
                table: "Tags",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Tags_CompanyPublicId_Epc",
                schema: "rfid",
                table: "Tags",
                columns: new[] { "CompanyPublicId", "Epc" },
                unique: true);
        }
    }
}
