using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ERP.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEdgeBusinessEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EdgeBusinessEvents",
                schema: "rfid",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyPublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    EdgeNodeId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    EventId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    EventType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SchemaVersion = table.Column<int>(type: "integer", nullable: false),
                    OccurredAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    OrderId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    EpcHex = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    MovementType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    FromZoneId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ToZoneId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ZoneId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ReaderId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    AntennaId = table.Column<int>(type: "integer", nullable: true),
                    Rssi = table.Column<decimal>(type: "numeric(8,3)", precision: 8, scale: 3, nullable: true),
                    Reason = table.Column<string>(type: "text", nullable: true),
                    MetadataJson = table.Column<string>(type: "text", nullable: false),
                    ReceivedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EdgeBusinessEvents", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EdgeBusinessEvents_CompanyPublicId_OccurredAtUtc",
                schema: "rfid",
                table: "EdgeBusinessEvents",
                columns: new[] { "CompanyPublicId", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_EdgeBusinessEvents_EdgeNodeId_OccurredAtUtc",
                schema: "rfid",
                table: "EdgeBusinessEvents",
                columns: new[] { "EdgeNodeId", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_EdgeBusinessEvents_EventId",
                schema: "rfid",
                table: "EdgeBusinessEvents",
                column: "EventId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EdgeBusinessEvents",
                schema: "rfid");
        }
    }
}
