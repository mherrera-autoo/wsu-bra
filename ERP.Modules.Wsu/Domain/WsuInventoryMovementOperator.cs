using ERP.Shared.Domain;

namespace ERP.Modules.Wsu.Domain;

public sealed class WsuInventoryMovementOperator : Entity
{
    public long InventoryMovementId { get; private set; }
    public WsuInventoryMovement InventoryMovement { get; private set; } = null!;
    public string CodigoOperador { get; private set; } = null!;
    public string? NombreOperador { get; private set; }
    public DateOnly? FechaMovimiento { get; private set; }
    public TimeOnly? HoraInicioMovimiento { get; private set; }
    public TimeOnly? HoraFinMovimiento { get; private set; }

    private WsuInventoryMovementOperator() { }

    public static WsuInventoryMovementOperator Create(
        long inventoryMovementId,
        string codigoOperador,
        string? nombreOperador,
        DateOnly? fechaMovimiento,
        TimeOnly? horaInicioMovimiento,
        TimeOnly? horaFinMovimiento)
    {
        if (inventoryMovementId <= 0) throw new ArgumentOutOfRangeException(nameof(inventoryMovementId));
        if (string.IsNullOrWhiteSpace(codigoOperador)) throw new ArgumentException("CodigoOperador is required.", nameof(codigoOperador));

        return new WsuInventoryMovementOperator
        {
            InventoryMovementId = inventoryMovementId,
            CodigoOperador = codigoOperador.Trim(),
            NombreOperador = NormalizeNullable(nombreOperador),
            FechaMovimiento = fechaMovimiento,
            HoraInicioMovimiento = horaInicioMovimiento,
            HoraFinMovimiento = horaFinMovimiento
        };
    }

    private static string? NormalizeNullable(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
