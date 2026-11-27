using Cinepolis.Application;
using Microsoft.AspNetCore.DataProtection;

namespace Cinepolis.Infrastructure;

/// <summary>Protege folios con el propósito compartido por los lectores de tickets.</summary>
public sealed class GeneradorQr(IDataProtectionProvider provider) : IGeneradorQr
{
    private readonly IDataProtector protector = provider.CreateProtector("Cinepolis.TicketQR.v1");

    /// <inheritdoc/>
    public string Generar(string folio) => protector.Protect(folio);
}
