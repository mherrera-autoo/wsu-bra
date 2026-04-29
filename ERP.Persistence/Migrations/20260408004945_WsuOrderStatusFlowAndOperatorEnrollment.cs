using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class WsuOrderStatusFlowAndOperatorEnrollment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_OrderItems_QuantityAvailable",
                schema: "wsu",
                table: "OrderItems");

            migrationBuilder.RenameColumn(
                name: "QuantityAvailable",
                schema: "wsu",
                table: "OrderItems",
                newName: "QuantityAvailableForFifo");

            migrationBuilder.RenameIndex(
                name: "IX_OrderItems_ProductPublicId_QuantityAvailable",
                schema: "wsu",
                table: "OrderItems",
                newName: "IX_OrderItems_ProductPublicId_QuantityAvailableForFifo");

            migrationBuilder.AddColumn<string>(
                name: "Code",
                schema: "rfid",
                table: "Operators",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE rfid."Operators"
                SET "Code" = COALESCE(NULLIF(TRIM("DocumentId"), ''), 'OP-' || "Id"::text)
                WHERE "Code" IS NULL OR TRIM("Code") = '';
                """);

            migrationBuilder.Sql(
                """
                WITH duplicates AS (
                    SELECT "Id", "CompanyPublicId", "Code",
                           ROW_NUMBER() OVER (PARTITION BY "CompanyPublicId", "Code" ORDER BY "Id") AS rn
                    FROM rfid."Operators"
                )
                UPDATE rfid."Operators" o
                SET "Code" = o."Code" || '-' || o."Id"::text
                FROM duplicates d
                WHERE o."Id" = d."Id" AND d.rn > 1;
                """);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                schema: "rfid",
                table: "Operators",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(64)",
                oldMaxLength: 64,
                oldNullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_OrderItems_QuantityAvailableForFifo",
                schema: "wsu",
                table: "OrderItems",
                sql: "\"QuantityAvailableForFifo\" >= 0 AND \"QuantityAvailableForFifo\" <= abs(\"VariacionStock\")");

            migrationBuilder.CreateIndex(
                name: "IX_Operators_CompanyPublicId_Code",
                schema: "rfid",
                table: "Operators",
                columns: new[] { "CompanyPublicId", "Code" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_OrderItems_QuantityAvailableForFifo",
                schema: "wsu",
                table: "OrderItems");

            migrationBuilder.DropIndex(
                name: "IX_Operators_CompanyPublicId_Code",
                schema: "rfid",
                table: "Operators");

            migrationBuilder.DropColumn(
                name: "Code",
                schema: "rfid",
                table: "Operators");

            migrationBuilder.RenameColumn(
                name: "QuantityAvailableForFifo",
                schema: "wsu",
                table: "OrderItems",
                newName: "QuantityAvailable");

            migrationBuilder.RenameIndex(
                name: "IX_OrderItems_ProductPublicId_QuantityAvailableForFifo",
                schema: "wsu",
                table: "OrderItems",
                newName: "IX_OrderItems_ProductPublicId_QuantityAvailable");

            migrationBuilder.AddCheckConstraint(
                name: "CK_OrderItems_QuantityAvailable",
                schema: "wsu",
                table: "OrderItems",
                sql: "\"QuantityAvailable\" >= 0 AND \"QuantityAvailable\" <= abs(\"VariacionStock\")");
        }
    }
}
