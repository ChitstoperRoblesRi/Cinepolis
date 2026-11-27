namespace Cinepolis.Domain;

/// <summary>Entidad de corte caja del sistema de cine.</summary>
public sealed class CorteCaja
{
    public int CorteId { get; init; }

    public decimal MontoEfectivoContado { get; init; }

    public decimal MontoReportadoSistema { get; init; }

    public decimal Diferencia { get; init; }

    public DateTime FechaHoraCierre { get; init; }

    public int SesionId { get; init; }
}
