using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddConsumptionEventCreatedByUserPublicId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserPublicId",
                schema: "wsu",
                table: "ConsumptionEvents",
                type: "uuid",
                nullable: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedByUserPublicId",
                schema: "wsu",
                table: "ConsumptionEvents");
        }
    }
}
