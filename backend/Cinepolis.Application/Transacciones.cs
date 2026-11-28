using System.ComponentModel.DataAnnotations;

namespace Cinepolis.Application;

/// <summary>Dirección a la que se enviará el código de acceso.</summary>
public sealed record SolicitarCodigo([Required, EmailAddress, MaxLength(150)] string Correo);
/// <summary>Correo y código presentados para iniciar sesión.</summary>
public sealed record Login([Required, EmailAddress, MaxLength(150)] string Correo,
    [Required, RegularExpression("[0-9]{6}")] string Codigo);
/// <summary>Datos de alta y código de verificación del nuevo socio.</summary>
public sealed record Registro([Required, EmailAddress, MaxLength(150)] string Correo,
    [Required, RegularExpression("[0-9]{6}")] string Codigo, [Required, MaxLength(100)] string UsuarioCc,
    [Required, MaxLength(100)] string Nombre, [Required, MaxLength(150)] string Apellidos,
    [Required] DateOnly? FechaNacimiento);
/// <summary>Datos del socio, nivel, saldo y progreso de visitas.</summary>
public sealed record PerfilClub(int SocioId, string UsuarioCc, string NumeroTarjeta, string Nombre,
    string Apellidos, string Correo, int PuntosAcumulados, DateOnly PuntosVencenFecha, int NivelId,
    string NombreNivel, decimal PorcentajeAcumulacionPuntos, int VisitasSemestreActual, string? SiguienteNivel, int? MetaVisitas);
/// <summary>Resumen de una compra del historial del socio.</summary>
public sealed record CompraClub(string Folio, string Titulo, string Complejo, DateTime Horario, decimal Total, string Estado, bool Activa);
/// <summary>Persiste códigos de acceso y verifica la identidad del socio.</summary>
public interface IAccesoClubRepository
{
    /// <summary>Registra un código si ha transcurrido el intervalo de reenvío.</summary>
    Task<bool> CrearCodigoAsync(string correo, string codigo, CancellationToken token);
    /// <summary>Valida el código y devuelve la identidad del socio.</summary>
    Task<int?> AutenticarAsync(Login login, Registro? registro, CancellationToken token);
}

/// <summary>Consulta perfiles e historial del programa Club.</summary>
public interface IClubQueryRepository
{
    /// <summary>Recupera el historial de compras del socio autenticado.</summary>
    Task<IEnumerable<CompraClub>> ComprasAsync(int socioId, CancellationToken token);
    /// <summary>Recupera los datos y beneficios de la cuenta del socio.</summary>
    Task<PerfilClub?> PerfilAsync(int socioId, CancellationToken token);
}

/// <summary>Asiento y tarifa solicitados para el checkout.</summary>
public sealed record BoletoCompra([Range(1, int.MaxValue)] int AsientoId, [Range(1, int.MaxValue)] int TipoBoletoId);
/// <summary>Cantidad, catálogo y opciones elegidas para una línea de dulcería.</summary>
public sealed record ProductoCompra([Range(1, int.MaxValue)] int ProductoId, [Range(1, 999)] int Cantidad,
    [Range(1, int.MaxValue)] int? CatalogoId = null, [MaxLength(100)] int[]? Opciones = null);
/// <summary>Forma de pago, datos del titular e importe esperado.</summary>
public sealed record PagoCompra([Required, RegularExpression("Tarjeta|PayPal|ClickToPay|Aplazo|Puntos")] string FormaPago, [MaxLength(100)] string? Nombre = null, [EmailAddress, MaxLength(150)] string? Correo = null,
    [MaxLength(20)] string? TarjetaClub = null, decimal? TotalEsperado = null);
/// <summary>Reservas, artículos y pago que deben confirmarse en una sola venta.</summary>
public sealed record Checkout([Range(1, int.MaxValue)] int? FuncionId, [MaxLength(2048)] string? TokenReserva,
    [Required, MaxLength(100)] BoletoCompra[] Asientos,
    [Required, MaxLength(100)] ProductoCompra[] Productos, [Required] PagoCompra Pago, [Range(1, int.MaxValue)] int? ComplejoId = null);
/// <summary>Butaca, función y precio reflejados en el comprobante.</summary>
public sealed record BoletoTicket(int FuncionId, int AsientoId, string Fila, int Columna, string? Numero,
    string TipoBoleto, decimal PrecioUnitario, string NombreSala, string NombreComercial, string Titulo, string Horario);
/// <summary>Artículo vendido con sus personalizaciones e importe unitario.</summary>
public sealed record ProductoTicket(int ProductoId, string NombreProducto, int Cantidad, decimal PrecioUnitario, string[] Opciones)
{
    /// <summary>Artículo vendido con sus personalizaciones e importe unitario.</summary>
    public ProductoTicket() : this(0, string.Empty, 0, 0m, []) { }
}
/// <summary>Folio, estado, pago e importes confirmados de la venta.</summary>
public sealed record CabeceraTicket(string Folio, string Estado, DateTime FechaVenta, string FormaPago,
    decimal CargoServicio, decimal ImporteTotal);
/// <summary>Comprobante de la venta con boletos, dulcería y puntos.</summary>
public sealed record Ticket(CabeceraTicket Orden, decimal SubtotalBoletos, decimal SubtotalDulceria,
    decimal PuntosAcreditados, IReadOnlyList<BoletoTicket> Boletos, IReadOnlyList<ProductoTicket> Productos);
/// <summary>Persiste compras confirmadas y recupera sus comprobantes.</summary>
public interface IOrdenRepository
{
    /// <summary>Lista los folios de las compras del titular.</summary>
    Task<IEnumerable<string>> FoliosAsync(int socioId, CancellationToken token);
    /// <summary>Recupera el ticket si el titular puede consultarlo.</summary>
    Task<Ticket?> LeerAsync(string folio, int? socioId, CancellationToken token);
}
