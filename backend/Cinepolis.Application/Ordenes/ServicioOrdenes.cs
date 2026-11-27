using Cinepolis.Domain;

namespace Cinepolis.Application;

/// <summary>Comprobante y contenido protegido que representa el folio en el QR.</summary>
public sealed record TicketDigital(Ticket Ticket, string QrPayload)
{
    public string Folio => Ticket.Orden.Folio;
    public string ValidacionQr => QrPayload;
}

/// <summary>Protege el identificador de venta para su representación como código QR.</summary>
public interface IGeneradorQr
{
    /// <summary>Emite un payload autenticado para el folio.</summary>
    string Generar(string folio);
}

/// <summary>Publica los asientos vendidos después de confirmar la orden.</summary>
public interface INotificadorVenta
{
    /// <summary>Comunica una venta confirmada a las salas correspondientes.</summary>
    Task PublicarAsync(Ticket ticket);
}

/// <summary>Coordina la publicación de ventas y la recuperación de comprobantes.</summary>
public sealed class ServicioOrdenes(IOrdenRepository ordenes, IGeneradorQr qr, INotificadorVenta notificador,
    IUnitOfWork unidad, OrdenCompraBuilder builder)
{
    /// <summary>Confirma la venta antes de notificar los asientos.</summary>
    public async Task<TicketDigital> ComprarAsync(Checkout compra, string conexion, int? socioId, CancellationToken token)
    {
        var digital = await unidad.EjecutarAsync(tx => ConfirmarAsync(tx, compra, conexion, socioId), token);
        await notificador.PublicarAsync(digital.Ticket);
        return digital;
    }

    private async Task<TicketDigital> ConfirmarAsync(ITransaccionOrden tx, Checkout compra, string conexion, int? socioId)
    {
        var disponibilidad = await tx.TomarReservasAsync(compra, conexion, socioId);
        var preparacion = builder.Iniciar(compra, disponibilidad.Bloqueados, disponibilidad.Convertidos);
        preparacion.ConBoletos(disponibilidad.Tarifas);
        foreach (var seleccion in await tx.CatalogoAsync(compra))
            preparacion.AgregarProducto(seleccion.Linea, seleccion.Producto);
        var orden = preparacion.Construir();

        var club = await tx.ClubAsync(socioId);
        var canje = compra.Pago.FormaPago == "Puntos";
        if (compra.Pago.TarjetaClub is string tarjeta && tarjeta != club?.NumeroTarjeta)
            throw new ConflictoCompra("Inicia sesión con la cuenta titular de la tarjeta Club.");
        if (canje && (club is null || !club.Vigente || club.Saldo < orden.Total))
            throw new ConflictoCompra("El saldo vigente de Club no cubre el total de la compra.");

        decimal? puntos = club is null ? null : canje ? orden.Total :
            decimal.Round(orden.Subtotal * club.Porcentaje / 100, 2, MidpointRounding.AwayFromZero);
        var ticket = await tx.GuardarAsync(orden, compra.Pago, socioId, puntos, canje ? "canje" : "acumulacion");
        return builder.Emitir(ticket);
    }

    /// <summary>Lista los folios del titular.</summary>
    public Task<IEnumerable<string>> FoliosAsync(int socioId, CancellationToken token) => ordenes.FoliosAsync(socioId, token);

    /// <summary>Recupera un comprobante y vuelve a proteger su folio.</summary>
    public async Task<TicketDigital?> LeerAsync(string folio, int? socioId, CancellationToken token)
    {
        var ticket = await ordenes.LeerAsync(folio, socioId, token);
        return ticket is null ? null : new(ticket, qr.Generar(folio));
    }
}

/// <summary>Verifica la estructura de las líneas antes del checkout.</summary>
public static class ValidadorCheckout
{
    /// <summary>Exige líneas válidas y una sola aparición de cada asiento.</summary>
    public static bool LineasValidas(Checkout compra) => compra.Asientos.All(b => b is not null)
        && compra.Productos.All(p => p is not null)
        && compra.Asientos.DistinctBy(b => b.AsientoId).Count() == compra.Asientos.Length;
}
