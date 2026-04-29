using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddWsuOrderEpcLoadConfirmedFlag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsEpcLoadConfirmed",
                schema: "wsu",
                table: "Orders",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql(
                """
                UPDATE wsu."Orders" o
                SET "IsEpcLoadConfirmed" = TRUE
                WHERE EXISTS (
                    SELECT 1
                    FROM wsu."OrderItemEpcAssignments" a
                    WHERE a."OrderId" = o."Id"
                );
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsEpcLoadConfirmed",
                schema: "wsu",
                table: "Orders");
        }
    }
}
