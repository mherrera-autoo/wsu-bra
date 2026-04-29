using ERP.Modules.Wsu.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Persistence.Configurations.Wsu;

public sealed class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    private const string Schema = "wsu";

    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("OrderItems", Schema, tableBuilder =>
        {
            tableBuilder.HasCheckConstraint("CK_OrderItems_VariacionStock", "\"VariacionStock\" <> 0");
            tableBuilder.HasCheckConstraint("CK_OrderItems_QuantityAvailableForFifo", "\"QuantityAvailableForFifo\" >= 0 AND \"QuantityAvailableForFifo\" <= abs(\"VariacionStock\")");
        });

        builder.HasKey(item => item.Id);

        builder.Property(item => item.PublicId).IsRequired();
        builder.Property(item => item.OrderId).IsRequired();
        builder.Property(item => item.ProductPublicId);
        builder.Property(item => item.QuantityAvailableForFifo).HasColumnType("numeric(18,3)").IsRequired();

        builder.Property(item => item.SkuWsu).HasMaxLength(128);
        builder.Property(item => item.NombreSkuWsu).HasMaxLength(256);
        builder.Property(item => item.SkuProveedor).HasMaxLength(128);
        builder.Property(item => item.NombreSkuProveedor).HasMaxLength(256);
        builder.Property(item => item.RutProveedor).HasMaxLength(32);
        builder.Property(item => item.RsProveedor).HasMaxLength(256);
        builder.Property(item => item.SkuCliente).HasMaxLength(128);
        builder.Property(item => item.NombreSkuCliente).HasMaxLength(256);
        builder.Property(item => item.RutCliente).HasMaxLength(32);
        builder.Property(item => item.RsCliente).HasMaxLength(256);
        builder.Property(item => item.Posicion).HasMaxLength(64);
        builder.Property(item => item.UnidadDeMedida).HasMaxLength(64);

        builder.Property(item => item.LoteMinimoCompra).HasColumnType("numeric(18,3)");
        builder.Property(item => item.LargoCompraCm).HasColumnType("numeric(18,3)");
        builder.Property(item => item.AnchoCompraCm).HasColumnType("numeric(18,3)");
        builder.Property(item => item.AltoCompraCm).HasColumnType("numeric(18,3)");
        builder.Property(item => item.PesoCompraKg).HasColumnType("numeric(18,3)");
        builder.Property(item => item.TipoAlmacenamientoCompra).HasMaxLength(64);

        builder.Property(item => item.LoteMinimoVenta).HasColumnType("numeric(18,3)");
        builder.Property(item => item.LargoVentaCm).HasColumnType("numeric(18,3)");
        builder.Property(item => item.AnchoVentaCm).HasColumnType("numeric(18,3)");
        builder.Property(item => item.AltoVentaCm).HasColumnType("numeric(18,3)");
        builder.Property(item => item.PesoVentaKg).HasColumnType("numeric(18,3)");
        builder.Property(item => item.TipoAlmacenamientoVenta).HasMaxLength(64);

        builder.Property(item => item.PrecioCompraUnitario).HasColumnType("numeric(18,2)");
        builder.Property(item => item.PrecioVentaUnitario).HasColumnType("numeric(18,2)");
        builder.Property(item => item.VariacionStock).HasColumnType("numeric(18,3)").IsRequired();
        builder.Property(item => item.StockMinimo).HasColumnType("numeric(18,3)");

        builder.HasIndex(item => item.PublicId).IsUnique();
        builder.HasIndex(item => item.OrderId);
        builder.HasIndex(item => new { item.OrderId, item.ProductPublicId });
        builder.HasIndex(item => new { item.ProductPublicId, item.QuantityAvailableForFifo });

        builder.HasMany(item => item.InventoryMovements)
            .WithOne(item => item.OrderItem)
            .HasForeignKey(item => item.OrderItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(item => item.OutboundConsumptions)
            .WithOne(item => item.OutOrderItem)
            .HasForeignKey(item => item.OutOrderItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(item => item.InboundConsumptions)
            .WithOne(item => item.InOrderItem)
            .HasForeignKey(item => item.InOrderItemId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
