using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace Cinepolis.Domain;

/// <summary>Identificador público de una venta web con 160 bits aleatorios.</summary>
public sealed record FolioCompra
{
    /// <summary>Prefijo de las ventas en línea en el historial.</summary>
    public const string Prefijo = "WEB-";

    private FolioCompra(string valor) => Valor = valor;

    /// <summary>Representación que se persiste y se protege para el QR.</summary>
    public string Valor { get; }

    /// <summary>Genera un folio con el formato aceptado por las rutas de consulta.</summary>
    public static FolioCompra Crear() => new(Prefijo + Convert.ToHexString(RandomNumberGenerator.GetBytes(20)));

    /// <summary>Verifica que el identificador tenga el formato público completo.</summary>
    public static bool EsValido(string valor) => Regex.IsMatch(valor, @"\AWEB-[0-9A-F]{40}\z");
}
