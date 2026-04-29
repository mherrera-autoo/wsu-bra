using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ChangeEdgeBusinessEventOrderIdToUuid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "ALTER TABLE rfid.\"EdgeBusinessEvents\" ALTER COLUMN \"OrderId\" TYPE uuid USING NULLIF(\"OrderId\", '')::uuid;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "ALTER TABLE rfid.\"EdgeBusinessEvents\" ALTER COLUMN \"OrderId\" TYPE character varying(128) USING \"OrderId\"::text;");
        }
    }
}
