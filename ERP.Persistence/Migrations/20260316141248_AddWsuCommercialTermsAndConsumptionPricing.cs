using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddWsuCommercialTermsAndConsumptionPricing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "TotalAmount",
                schema: "wsu",
                table: "ConsumptionEvents",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "UnitPrice",
                schema: "wsu",
                table: "ConsumptionEvents",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastUnitSalePriceUpdated",
                schema: "wsu",
                table: "ClientProductWarehouses",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MinimumSaleLot",
                schema: "wsu",
                table: "ClientProductWarehouses",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 1m);

            migrationBuilder.AddColumn<decimal>(
                name: "UnitSalePrice",
                schema: "wsu",
                table: "ClientProductWarehouses",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<decimal>(
                name: "TotalAmount",
                schema: "wsu",
                table: "ConsumptionEvents",
                type: "numeric(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldDefaultValue: 0m);

            migrationBuilder.AlterColumn<decimal>(
                name: "UnitPrice",
                schema: "wsu",
                table: "ConsumptionEvents",
                type: "numeric(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldDefaultValue: 0m);

            migrationBuilder.AlterColumn<decimal>(
                name: "MinimumSaleLot",
                schema: "wsu",
                table: "ClientProductWarehouses",
                type: "numeric(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldDefaultValue: 1m);

            migrationBuilder.AlterColumn<decimal>(
                name: "UnitSalePrice",
                schema: "wsu",
                table: "ClientProductWarehouses",
                type: "numeric(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldDefaultValue: 0m);

            migrationBuilder.AddCheckConstraint(
                name: "CK_ConsumptionEvents_TotalAmount",
                schema: "wsu",
                table: "ConsumptionEvents",
                sql: "\"TotalAmount\" >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ConsumptionEvents_UnitPrice",
                schema: "wsu",
                table: "ConsumptionEvents",
                sql: "\"UnitPrice\" >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ClientProductWarehouses_MinimumSaleLot",
                schema: "wsu",
                table: "ClientProductWarehouses",
                sql: "\"MinimumSaleLot\" > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ClientProductWarehouses_UnitSalePrice",
                schema: "wsu",
                table: "ClientProductWarehouses",
                sql: "\"UnitSalePrice\" >= 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_ConsumptionEvents_TotalAmount",
                schema: "wsu",
                table: "ConsumptionEvents");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ConsumptionEvents_UnitPrice",
                schema: "wsu",
                table: "ConsumptionEvents");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ClientProductWarehouses_MinimumSaleLot",
                schema: "wsu",
                table: "ClientProductWarehouses");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ClientProductWarehouses_UnitSalePrice",
                schema: "wsu",
                table: "ClientProductWarehouses");

            migrationBuilder.DropColumn(
                name: "TotalAmount",
                schema: "wsu",
                table: "ConsumptionEvents");

            migrationBuilder.DropColumn(
                name: "UnitPrice",
                schema: "wsu",
                table: "ConsumptionEvents");

            migrationBuilder.DropColumn(
                name: "LastUnitSalePriceUpdated",
                schema: "wsu",
                table: "ClientProductWarehouses");

            migrationBuilder.DropColumn(
                name: "MinimumSaleLot",
                schema: "wsu",
                table: "ClientProductWarehouses");

            migrationBuilder.DropColumn(
                name: "UnitSalePrice",
                schema: "wsu",
                table: "ClientProductWarehouses");
        }
    }
}
