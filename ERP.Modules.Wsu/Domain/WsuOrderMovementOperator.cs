using ERP.Shared.Domain;

namespace ERP.Modules.Wsu.Domain;

public sealed class WsuOrderMovementOperator : Entity
{
    public long OrderId { get; private set; }
    public Order Order { get; private set; } = null!;
    public string CodigoOperador { get; private set; } = null!;
    public string? NombreOperador { get; private set; }
    public DateOnly? FechaMovimiento { get; private set; }
    public TimeOnly? HoraInicioMovimiento { get; private set; }
    public TimeOnly? HoraFinMovimiento { get; private set; }

    private WsuOrderMovementOperator() { }

    public static WsuOrderMovementOperator Create(
        long orderId,
        string codigoOperador,
        string? nombreOperador,
        DateOnly? fechaMovimiento,
        TimeOnly? horaInicioMovimiento,
        TimeOnly? horaFinMovimiento)
    {
        if (orderId <= 0) throw new ArgumentOutOfRangeException(nameof(orderId));
        if (string.IsNullOrWhiteSpace(codigoOperador)) throw new ArgumentException("CodigoOperador is required.", nameof(codigoOperador));

        return new WsuOrderMovementOperator
        {
            OrderId = orderId,
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
