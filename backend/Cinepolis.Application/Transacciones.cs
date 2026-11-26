using System.ComponentModel.DataAnnotations;

namespace Cinepolis.Application;

public sealed record SolicitarCodigo([Required, EmailAddress, MaxLength(150)] string Correo);
public sealed record Login([Required, EmailAddress, MaxLength(150)] string Correo,
    [Required, RegularExpression("[0-9]{6}")] string Codigo);
public sealed record Registro([Required, EmailAddress, MaxLength(150)] string Correo,
    [Required, RegularExpression("[0-9]{6}")] string Codigo, [Required, MaxLength(100)] string UsuarioCc,
    [Required, MaxLength(100)] string Nombre, [Required, MaxLength(150)] string Apellidos,
    [Required] DateOnly? FechaNacimiento);
public sealed record PerfilClub(int SocioId, string UsuarioCc, string NumeroTarjeta, string Nombre,
    string Apellidos, string Correo, int PuntosAcumulados, DateOnly PuntosVencenFecha, int NivelId,
    string NombreNivel, decimal PorcentajeAcumulacionPuntos, int VisitasSemestreActual, string? SiguienteNivel, int? MetaVisitas);
public sealed record CompraClub(string Folio, string Titulo, string Complejo, DateTime Horario, decimal Total, string Estado, bool Activa);
public interface IClubRepository
{
    Task<bool> CrearCodigoAsync(string correo, string codigo, CancellationToken token);
    Task<int?> AutenticarAsync(Login login, Registro? registro, CancellationToken token);
    Task<IEnumerable<CompraClub>> ComprasAsync(int socioId, CancellationToken token);
    Task<PerfilClub?> PerfilAsync(int socioId, CancellationToken token);
}

public sealed record BoletoCompra([Range(1, int.MaxValue)] int AsientoId, [Range(1, int.MaxValue)] int TipoBoletoId);
public sealed record ProductoCompra([Range(1, int.MaxValue)] int ProductoId, [Range(1, 999)] int Cantidad,
    [Range(1, int.MaxValue)] int? CatalogoId = null, [MaxLength(100)] int[]? Opciones = null);
public sealed record PagoCompra([Required, RegularExpression("Tarjeta|PayPal|ClickToPay|Aplazo|Puntos")] string FormaPago, [MaxLength(100)] string? Nombre = null, [EmailAddress, MaxLength(150)] string? Correo = null,
    [MaxLength(20)] string? TarjetaClub = null, decimal? TotalEsperado = null);
public sealed record Checkout([Range(1, int.MaxValue)] int FuncionId, [Required, MaxLength(2048)] string TokenReserva,
    [Required, MinLength(1), MaxLength(100)] BoletoCompra[] Asientos,
    [Required, MaxLength(100)] ProductoCompra[] Productos, [Required] PagoCompra Pago);
public sealed record BoletoTicket(int FuncionId, int AsientoId, string Fila, int Columna, string? Numero,
    string TipoBoleto, decimal PrecioUnitario, string NombreSala, string NombreComercial, string Titulo, string Horario);
public sealed record ProductoTicket(int ProductoId, string NombreProducto, int Cantidad, decimal PrecioUnitario, string[] Opciones)
{
    public ProductoTicket() : this(0, string.Empty, 0, 0m, []) { }
}
public sealed record CabeceraTicket(string Folio, string Estado, DateTime FechaVenta, string FormaPago,
    decimal CargoServicio, decimal ImporteTotal);
public sealed record Ticket(CabeceraTicket Orden, decimal SubtotalBoletos, decimal SubtotalDulceria,
    decimal PuntosAcreditados, IReadOnlyList<BoletoTicket> Boletos, IReadOnlyList<ProductoTicket> Productos);
public interface IOrdenRepository
{
    Task<Ticket> ComprarAsync(Checkout compra, string conexion, int? socioId, CancellationToken token);
    Task<IEnumerable<string>> FoliosAsync(int socioId, CancellationToken token);
    Task<Ticket?> LeerAsync(string folio, int? socioId, CancellationToken token);
}
public sealed class ConflictoCompra(string mensaje) : Exception(mensaje);
