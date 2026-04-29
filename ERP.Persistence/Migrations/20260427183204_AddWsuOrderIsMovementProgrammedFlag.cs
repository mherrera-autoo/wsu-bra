using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddWsuOrderIsMovementProgrammedFlag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsMovementProgrammed",
                schema: "wsu",
                table: "Orders",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsMovementProgrammed",
                schema: "wsu",
                table: "Orders");
        }
    }
}
