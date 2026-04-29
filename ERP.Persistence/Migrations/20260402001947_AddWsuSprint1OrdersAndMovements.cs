using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ERP.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddWsuSprint1OrdersAndMovements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_ClientProductWarehouses_PlannedInitialStock",
                schema: "wsu",
                table: "ClientProductWarehouses");

            migrationBuilder.DropColumn(
                name: "PlannedInitialStock",
                schema: "wsu",
                table: "ClientProductWarehouses");

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "Warehouses",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Position",
                table: "Warehouses",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsStackable",
                table: "Products",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "LengthCm",
                table: "Products",
                type: "numeric(18,3)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StorageType",
                table: "Products",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "WeightKg",
                table: "Products",
                type: "numeric(18,3)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "WidthCm",
                table: "Products",
                type: "numeric(18,3)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsStackable",
                schema: "wsu",
                table: "ClientProductWarehouses",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "LengthCm",
                schema: "wsu",
                table: "ClientProductWarehouses",
                type: "numeric(18,3)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StorageType",
                schema: "wsu",
                table: "ClientProductWarehouses",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "WeightKg",
                schema: "wsu",
                table: "ClientProductWarehouses",
                type: "numeric(18,3)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "WidthCm",
                schema: "wsu",
                table: "ClientProductWarehouses",
                type: "numeric(18,3)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Orders",
                schema: "wsu",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyPublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    WarehousePublicId = table.Column<Guid>(type: "uuid", nullable: true),
                    OrderType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    OrderNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ExternalOrderNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    MovementDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserPublicId = table.Column<Guid>(type: "uuid", nullable: true),
                    SourceType = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OrderItems",
                schema: "wsu",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrderId = table.Column<long>(type: "bigint", nullable: false),
                    ProductPublicId = table.Column<Guid>(type: "uuid", nullable: true),
                    Quantity = table.Column<decimal>(type: "numeric(18,3)", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    LineTotal = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    SkuWsu = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    NombreSkuWsu = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    SkuProveedor = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    NombreSkuProveedor = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    RutProveedor = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    RsProveedor = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    SkuCliente = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    NombreSkuCliente = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    RutCliente = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    RsCliente = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    CodigoBodega = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    UbicacionBodega = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Posicion = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    UnidadDeMedida = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    LoteMinimoCompra = table.Column<decimal>(type: "numeric(18,3)", nullable: true),
                    LargoCompraCm = table.Column<decimal>(type: "numeric(18,3)", nullable: true),
                    AnchoCompraCm = table.Column<decimal>(type: "numeric(18,3)", nullable: true),
                    AltoCompraCm = table.Column<decimal>(type: "numeric(18,3)", nullable: true),
                    PesoCompraKg = table.Column<decimal>(type: "numeric(18,3)", nullable: true),
                    ApilableCompra = table.Column<bool>(type: "boolean", nullable: true),
                    TipoAlmacenamientoCompra = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    LoteMinimoVenta = table.Column<decimal>(type: "numeric(18,3)", nullable: true),
                    LargoVentaCm = table.Column<decimal>(type: "numeric(18,3)", nullable: true),
                    AnchoVentaCm = table.Column<decimal>(type: "numeric(18,3)", nullable: true),
                    AltoVentaCm = table.Column<decimal>(type: "numeric(18,3)", nullable: true),
                    PesoVentaKg = table.Column<decimal>(type: "numeric(18,3)", nullable: true),
                    ApilableVenta = table.Column<bool>(type: "boolean", nullable: true),
                    TipoAlmacenamientoVenta = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    PrecioCompraUnitario = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    PrecioVentaUnitario = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    VariacionStock = table.Column<decimal>(type: "numeric(18,3)", nullable: false),
                    StockMinimo = table.Column<decimal>(type: "numeric(18,3)", nullable: true),
                    CodigoOperador = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    NombreOperador = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    TipoOrden = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    FechaMovimiento = table.Column<DateOnly>(type: "date", nullable: true),
                    HoraInicioMovimiento = table.Column<TimeOnly>(type: "time", nullable: true),
                    HoraFinMovimiento = table.Column<TimeOnly>(type: "time", nullable: true),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItems", x => x.Id);
                    table.CheckConstraint("CK_OrderItems_Quantity", "\"Quantity\" > 0");
                    table.ForeignKey(
                        name: "FK_OrderItems_Orders_OrderId",
                        column: x => x.OrderId,
                        principalSchema: "wsu",
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InventoryMovements",
                schema: "wsu",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyPublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    WarehousePublicId = table.Column<Guid>(type: "uuid", nullable: true),
                    OrderId = table.Column<long>(type: "bigint", nullable: false),
                    OrderItemId = table.Column<long>(type: "bigint", nullable: false),
                    ProductPublicId = table.Column<Guid>(type: "uuid", nullable: true),
                    MovementType = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,3)", nullable: false),
                    SignedQuantity = table.Column<decimal>(type: "numeric(18,3)", nullable: false),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    PerformedByUserPublicId = table.Column<Guid>(type: "uuid", nullable: true),
                    SourceType = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryMovements1", x => x.Id);
                    table.CheckConstraint("CK_WsuInventoryMovements_Quantity", "\"Quantity\" > 0");
                    table.CheckConstraint("CK_WsuInventoryMovements_SignedQuantity", "\"SignedQuantity\" <> 0");
                    table.ForeignKey(
                        name: "FK_InventoryMovements_OrderItems_OrderItemId",
                        column: x => x.OrderItemId,
                        principalSchema: "wsu",
                        principalTable: "OrderItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InventoryMovements_Orders_OrderId",
                        column: x => x.OrderId,
                        principalSchema: "wsu",
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.AddCheckConstraint(
                name: "CK_ClientProductWarehouses_LengthCm",
                schema: "wsu",
                table: "ClientProductWarehouses",
                sql: "\"LengthCm\" IS NULL OR \"LengthCm\" >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ClientProductWarehouses_WeightKg",
                schema: "wsu",
                table: "ClientProductWarehouses",
                sql: "\"WeightKg\" IS NULL OR \"WeightKg\" >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ClientProductWarehouses_WidthCm",
                schema: "wsu",
                table: "ClientProductWarehouses",
                sql: "\"WidthCm\" IS NULL OR \"WidthCm\" >= 0");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryMovements_CompanyPublicId_OccurredAt",
                schema: "wsu",
                table: "InventoryMovements",
                columns: new[] { "CompanyPublicId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_InventoryMovements_CompanyPublicId_ProductPublicId_Occurred~",
                schema: "wsu",
                table: "InventoryMovements",
                columns: new[] { "CompanyPublicId", "ProductPublicId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_InventoryMovements_OrderId",
                schema: "wsu",
                table: "InventoryMovements",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryMovements_OrderItemId",
                schema: "wsu",
                table: "InventoryMovements",
                column: "OrderItemId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryMovements_PublicId",
                schema: "wsu",
                table: "InventoryMovements",
                column: "PublicId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_OrderId",
                schema: "wsu",
                table: "OrderItems",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_OrderId_ProductPublicId",
                schema: "wsu",
                table: "OrderItems",
                columns: new[] { "OrderId", "ProductPublicId" });

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_OrderId_TipoOrden",
                schema: "wsu",
                table: "OrderItems",
                columns: new[] { "OrderId", "TipoOrden" });

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_PublicId",
                schema: "wsu",
                table: "OrderItems",
                column: "PublicId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CompanyPublicId_MovementDate",
                schema: "wsu",
                table: "Orders",
                columns: new[] { "CompanyPublicId", "MovementDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CompanyPublicId_OrderNumber",
                schema: "wsu",
                table: "Orders",
                columns: new[] { "CompanyPublicId", "OrderNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CompanyPublicId_OrderType_Status",
                schema: "wsu",
                table: "Orders",
                columns: new[] { "CompanyPublicId", "OrderType", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_PublicId",
                schema: "wsu",
                table: "Orders",
                column: "PublicId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InventoryMovements",
                schema: "wsu");

            migrationBuilder.DropTable(
                name: "OrderItems",
                schema: "wsu");

            migrationBuilder.DropTable(
                name: "Orders",
                schema: "wsu");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ClientProductWarehouses_LengthCm",
                schema: "wsu",
                table: "ClientProductWarehouses");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ClientProductWarehouses_WeightKg",
                schema: "wsu",
                table: "ClientProductWarehouses");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ClientProductWarehouses_WidthCm",
                schema: "wsu",
                table: "ClientProductWarehouses");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "Warehouses");

            migrationBuilder.DropColumn(
                name: "Position",
                table: "Warehouses");

            migrationBuilder.DropColumn(
                name: "IsStackable",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "LengthCm",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "StorageType",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "WeightKg",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "WidthCm",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "IsStackable",
                schema: "wsu",
                table: "ClientProductWarehouses");

            migrationBuilder.DropColumn(
                name: "LengthCm",
                schema: "wsu",
                table: "ClientProductWarehouses");

            migrationBuilder.DropColumn(
                name: "StorageType",
                schema: "wsu",
                table: "ClientProductWarehouses");

            migrationBuilder.DropColumn(
                name: "WeightKg",
                schema: "wsu",
                table: "ClientProductWarehouses");

            migrationBuilder.DropColumn(
                name: "WidthCm",
                schema: "wsu",
                table: "ClientProductWarehouses");

            migrationBuilder.AddColumn<decimal>(
                name: "PlannedInitialStock",
                schema: "wsu",
                table: "ClientProductWarehouses",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddCheckConstraint(
                name: "CK_ClientProductWarehouses_PlannedInitialStock",
                schema: "wsu",
                table: "ClientProductWarehouses",
                sql: "\"PlannedInitialStock\" >= 0");
        }
    }
}
