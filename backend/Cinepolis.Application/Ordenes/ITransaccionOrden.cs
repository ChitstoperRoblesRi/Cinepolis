namespace Cinepolis.Application;

/// <summary>Butacas bloqueadas, reservas convertidas y tarifas verificables en una misma transacción.</summary>
public sealed record DisponibilidadCompra(IReadOnlyList<int> Bloqueados, IReadOnlyList<int> Convertidos, IReadOnlyList<Tarifa> Tarifas);
/// <summary>Línea solicitada junto con su presentación vigente en el complejo.</summary>
public sealed record SeleccionCatalogo(ProductoCompra Linea, ProductoDto Producto);
/// <summary>Saldo y condiciones de la cuenta bloqueada para la venta.</summary>
public sealed record SaldoClub(string NumeroTarjeta, int Saldo, decimal Porcentaje, bool Vigente);

/// <summary>Operaciones disponibles dentro de una única transacción de checkout.</summary>
public interface ITransaccionOrden
{
    /// <summary>Bloquea las butacas en orden y convierte únicamente las reservas del titular.</summary>
    Task<DisponibilidadCompra> TomarReservasAsync(Checkout compra, string conexion, int? socioId);
    /// <summary>Resuelve cada selección en el catálogo correspondiente al complejo de la función.</summary>
    Task<IReadOnlyList<SeleccionCatalogo>> CatalogoAsync(Checkout compra);
    /// <summary>Bloquea la cuenta y consulta su saldo y vigencia.</summary>
    Task<SaldoClub?> ClubAsync(int? socioId);
    /// <summary>Persiste la venta completa y recupera su comprobante.</summary>
    Task<Ticket> GuardarAsync(OrdenCotizada orden, PagoCompra pago, int? socioId, decimal? puntos, string movimiento);
}
