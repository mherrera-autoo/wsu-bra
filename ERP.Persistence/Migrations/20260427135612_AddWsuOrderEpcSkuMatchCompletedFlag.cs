using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddWsuOrderEpcSkuMatchCompletedFlag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsEpcSkuMatchCompleted",
                schema: "wsu",
                table: "Orders",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql(
                """
                UPDATE wsu."Orders" o
                SET "IsEpcSkuMatchCompleted" = TRUE
                WHERE EXISTS (
                    SELECT 1
                    FROM wsu."OrderItemEpcAssignments" a
                    WHERE a."OrderId" = o."Id"
                )
                AND NOT EXISTS (
                    SELECT 1
                    FROM wsu."OrderItemEpcAssignments" a
                    WHERE a."OrderId" = o."Id"
                      AND a."IsChecked" = FALSE
                );
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsEpcSkuMatchCompleted",
                schema: "wsu",
                table: "Orders");
        }
    }
}
