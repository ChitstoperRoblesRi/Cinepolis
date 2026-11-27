namespace Cinepolis.Domain;

/// <summary>La compra solicitada ya no puede completarse en las condiciones indicadas.</summary>
public sealed class ConflictoCompra(string mensaje) : Exception(mensaje);
