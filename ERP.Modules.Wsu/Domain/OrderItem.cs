using ERP.Shared.Domain;

namespace ERP.Modules.Wsu.Domain;

public sealed class OrderItem : Entity
{
    public long OrderId { get; private set; }
    public Order Order { get; private set; } = null!;
    public Guid? ProductPublicId { get; private set; }
    public decimal QuantityAvailableForFifo { get; private set; }

    public string? SkuWsu { get; private set; }
    public string? NombreSkuWsu { get; private set; }
    public string? SkuProveedor { get; private set; }
    public string? NombreSkuProveedor { get; private set; }
    public string? RutProveedor { get; private set; }
    public string? RsProveedor { get; private set; }
    public string? SkuCliente { get; private set; }
    public string? NombreSkuCliente { get; private set; }
    public string? RutCliente { get; private set; }
    public string? RsCliente { get; private set; }
    public string? Posicion { get; private set; }
    public string? UnidadDeMedida { get; private set; }
    public decimal? LoteMinimoCompra { get; private set; }
    public decimal? LargoCompraCm { get; private set; }
    public decimal? AnchoCompraCm { get; private set; }
    public decimal? AltoCompraCm { get; private set; }
    public decimal? PesoCompraKg { get; private set; }
    public bool? ApilableCompra { get; private set; }
    public string? TipoAlmacenamientoCompra { get; private set; }
    public decimal? LoteMinimoVenta { get; private set; }
    public decimal? LargoVentaCm { get; private set; }
    public decimal? AnchoVentaCm { get; private set; }
    public decimal? AltoVentaCm { get; private set; }
    public decimal? PesoVentaKg { get; private set; }
    public bool? ApilableVenta { get; private set; }
    public string? TipoAlmacenamientoVenta { get; private set; }
    public decimal? PrecioCompraUnitario { get; private set; }
    public decimal? PrecioVentaUnitario { get; private set; }
    public decimal VariacionStock { get; private set; }
    public decimal? StockMinimo { get; private set; }

    public ICollection<WsuInventoryMovement> InventoryMovements { get; private set; } = new List<WsuInventoryMovement>();
    public ICollection<OrderItemConsumption> OutboundConsumptions { get; private set; } = new List<OrderItemConsumption>();
    public ICollection<OrderItemConsumption> InboundConsumptions { get; private set; } = new List<OrderItemConsumption>();

    private OrderItem() { }

    public static OrderItem Create(
        long orderId,
        Guid? productPublicId,
        decimal quantityAvailable,
        string? skuWsu,
        string? nombreSkuWsu,
        string? skuProveedor,
        string? nombreSkuProveedor,
        string? rutProveedor,
        string? rsProveedor,
        string? skuCliente,
        string? nombreSkuCliente,
        string? rutCliente,
        string? rsCliente,
        string? posicion,
        string? unidadDeMedida,
        decimal? loteMinimoCompra,
        decimal? largoCompraCm,
        decimal? anchoCompraCm,
        decimal? altoCompraCm,
        decimal? pesoCompraKg,
        bool? apilableCompra,
        string? tipoAlmacenamientoCompra,
        decimal? loteMinimoVenta,
        decimal? largoVentaCm,
        decimal? anchoVentaCm,
        decimal? altoVentaCm,
        decimal? pesoVentaKg,
        bool? apilableVenta,
        string? tipoAlmacenamientoVenta,
        decimal? precioCompraUnitario,
        decimal? precioVentaUnitario,
        decimal variacionStock,
        decimal? stockMinimo)
    {
        if (orderId <= 0) throw new ArgumentOutOfRangeException(nameof(orderId));
        if (variacionStock == 0m) throw new ArgumentOutOfRangeException(nameof(variacionStock));
        if (quantityAvailable < 0m) throw new ArgumentOutOfRangeException(nameof(quantityAvailable));
        if (quantityAvailable > Math.Abs(variacionStock)) throw new ArgumentOutOfRangeException(nameof(quantityAvailable));

        return new OrderItem
        {
            OrderId = orderId,
            ProductPublicId = productPublicId,
            QuantityAvailableForFifo = quantityAvailable,
            SkuWsu = NormalizeNullable(skuWsu),
            NombreSkuWsu = NormalizeNullable(nombreSkuWsu),
            SkuProveedor = NormalizeNullable(skuProveedor),
            NombreSkuProveedor = NormalizeNullable(nombreSkuProveedor),
            RutProveedor = NormalizeNullable(rutProveedor),
            RsProveedor = NormalizeNullable(rsProveedor),
            SkuCliente = NormalizeNullable(skuCliente),
            NombreSkuCliente = NormalizeNullable(nombreSkuCliente),
            RutCliente = NormalizeNullable(rutCliente),
            RsCliente = NormalizeNullable(rsCliente),
            Posicion = NormalizeNullable(posicion),
            UnidadDeMedida = NormalizeNullable(unidadDeMedida),
            LoteMinimoCompra = loteMinimoCompra,
            LargoCompraCm = largoCompraCm,
            AnchoCompraCm = anchoCompraCm,
            AltoCompraCm = altoCompraCm,
            PesoCompraKg = pesoCompraKg,
            ApilableCompra = apilableCompra,
            TipoAlmacenamientoCompra = NormalizeNullable(tipoAlmacenamientoCompra),
            LoteMinimoVenta = loteMinimoVenta,
            LargoVentaCm = largoVentaCm,
            AnchoVentaCm = anchoVentaCm,
            AltoVentaCm = altoVentaCm,
            PesoVentaKg = pesoVentaKg,
            ApilableVenta = apilableVenta,
            TipoAlmacenamientoVenta = NormalizeNullable(tipoAlmacenamientoVenta),
            PrecioCompraUnitario = precioCompraUnitario,
            PrecioVentaUnitario = precioVentaUnitario,
            VariacionStock = variacionStock,
            StockMinimo = stockMinimo
        };
    }

    public decimal GetMovementQuantity() => Math.Abs(VariacionStock);

    public decimal GetPendingReconciliationQuantity() => GetMovementQuantity() - QuantityAvailableForFifo;

    public void ReconcileInboundQuantity(decimal quantity)
    {
        if (quantity <= 0m) throw new ArgumentOutOfRangeException(nameof(quantity));
        var newAvailable = QuantityAvailableForFifo + quantity;
        if (newAvailable > GetMovementQuantity())
        {
            throw new InvalidOperationException("Reconciled quantity exceeds inbound order item quantity.");
        }

        QuantityAvailableForFifo = newAvailable;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ConsumeQuantity(decimal quantity)
    {
        if (quantity <= 0m) throw new ArgumentOutOfRangeException(nameof(quantity));
        if (quantity > QuantityAvailableForFifo) throw new InvalidOperationException("Insufficient available quantity in inbound order item.");

        QuantityAvailableForFifo -= quantity;
        UpdatedAt = DateTime.UtcNow;
    }

    private static string? NormalizeNullable(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
