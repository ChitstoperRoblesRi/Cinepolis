using Cinepolis.Domain;

namespace Cinepolis.Application;

/// <summary>Precio vigente de un boleto para una butaca bloqueada.</summary>
public sealed record Tarifa(int AsientoId, int TipoBoletoId, decimal Precio, decimal Cargo);
/// <summary>Línea de dulcería valorada según su catálogo y selección.</summary>
public sealed record Articulo(int ProductoId, int Cantidad, decimal Precio, int[] Opciones);
/// <summary>Importes confirmados y folio que se persistirán juntos.</summary>
public sealed record OrdenCotizada(decimal Cargo, decimal Subtotal, decimal Total, string Folio,
    IReadOnlyList<Tarifa> Boletos, IReadOnlyList<Articulo> Productos)
{
    public int? ComplejoId { get; init; }
}

/// <summary>Valida las etapas de reserva, selección y cotización antes de emitir el folio.</summary>
public sealed class OrdenCompraBuilder(ICotizadorDulceria calculadora, IGeneradorQr qr, IValidadorSeleccionDulceria seleccion)
{
    /// <summary>Exige una reserva propia y vigente por cada asiento solicitado.</summary>
    public Preparacion Iniciar(Checkout compra, IReadOnlyList<int> bloqueados, IReadOnlyList<int> convertidos)
    {
        var ids = compra.Asientos.Select(b => b.AsientoId).ToHashSet();
        if (!ValidadorCheckout.ModalidadValida(compra) || ids.Count != compra.Asientos.Length || !ids.SetEquals(bloqueados) ||
            convertidos.Count != ids.Count || !ids.SetEquals(convertidos))
            throw new ConflictoCompra("Uno o más asientos carecen de una reserva vigente de esta sesión.");
        return new Preparacion(this, compra);
    }

    /// <summary>Mantiene las líneas verificadas de un único intento de compra.</summary>
    public sealed class Preparacion
    {
        private readonly OrdenCompraBuilder builder;
        private readonly Checkout compra;
        private readonly List<Articulo> productos = [];
        private readonly List<ProductoCompra> pendientes;
        private IReadOnlyList<Tarifa> boletos = [];

        internal Preparacion(OrdenCompraBuilder builder, Checkout compra)
        {
            this.builder = builder;
            this.compra = compra;
            pendientes = compra.Productos.ToList();
        }

        /// <summary>Asocia cada asiento con la tarifa vigente de su función.</summary>
        public void ConBoletos(IReadOnlyList<Tarifa> tarifas)
        {
            var solicitados = compra.Asientos.Select(b => (b.AsientoId, b.TipoBoletoId)).ToHashSet();
            if (tarifas.Count != solicitados.Count || !solicitados.SetEquals(tarifas.Select(t => (t.AsientoId, t.TipoBoletoId))))
                throw new ConflictoCompra("Función o tarifa de boleto no disponible.");
            boletos = tarifas.ToArray();
        }

        /// <summary>Agrega el artículo con su precio y opciones verificadas.</summary>
        public void AgregarProducto(ProductoCompra linea, ProductoDto producto)
        {
            if (!pendientes.Contains(linea) || linea.ProductoId != producto.ProductoId || linea.Cantidad <= 0)
                throw new ConflictoCompra("Producto no disponible en este complejo.");
            productos.Add(builder.Cotizar(linea, producto));
            pendientes.Remove(linea);
        }

        /// <summary>Emite una orden únicamente después de reunir todas las líneas.</summary>
        public OrdenCotizada Construir()
        {
            if (boletos.Count != compra.Asientos.Length || pendientes.Count != 0)
                throw new ConflictoCompra("Faltan líneas para completar la orden.");
            return Totalizar(boletos, productos.ToArray(), compra.Pago.TotalEsperado) with { ComplejoId = compra.FuncionId is null ? compra.ComplejoId : null };
        }
    }

    /// <summary>Valida las cantidades por componente y cotiza las opciones seleccionadas.</summary>
    private Articulo Cotizar(ProductoCompra linea, ProductoDto p)
    {
        var ids = linea.Opciones ?? [];
        var opciones = seleccion.Validar(p, ids);
        var precio = calculadora.Calcular(p, opciones);
        if (precio is < 0m or > 999999.99m) throw new ConflictoCompra("Importe de artículo fuera de rango.");
        return new(p.ProductoId, linea.Cantidad, precio, opciones.Where(o => o.OpcionId > 0).Select(o => o.OpcionId).ToArray());
    }

    /// <summary>Contrasta el total con el autorizado antes de asignar el folio digital.</summary>
    private static OrdenCotizada Totalizar(IReadOnlyList<Tarifa> boletos, IReadOnlyList<Articulo> productos, decimal? totalEsperado)
    {
        decimal cargo = boletos.Sum(b => b.Cargo);
        decimal subtotal = boletos.Sum(b => b.Precio) + productos.Sum(p => p.Cantidad * p.Precio);
        decimal total = subtotal + cargo;
        if (total > 99999999.99m || cargo > 999999.99m) throw new ConflictoCompra("El importe supera el máximo permitido.");
        if (totalEsperado is decimal esperado && esperado != total)
            throw new ConflictoCompra("El precio cambió. Revisa las tarifas y la dulcería antes de confirmar.");
        return new(cargo, subtotal, total, FolioCompra.Crear().Valor, boletos, productos);
    }

    /// <summary>Genera el comprobante digital dentro de la operación de compra.</summary>
    public TicketDigital Emitir(Ticket ticket) => new(ticket, qr.Generar(ticket.Orden.Folio));
}
