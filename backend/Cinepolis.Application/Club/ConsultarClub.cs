namespace Cinepolis.Application;

/// <summary>Consulta el historial y perfil de la cuenta autenticada.</summary>
public sealed class ConsultarClub(IClubQueryRepository club)
{
    /// <summary>Recupera las compras asociadas al socio.</summary>
    public Task<IEnumerable<CompraClub>> ComprasAsync(int socio, CancellationToken token) => club.ComprasAsync(socio, token);

    /// <summary>Recupera saldo, nivel y datos de perfil del titular.</summary>
    public Task<PerfilClub?> PerfilAsync(int socio, CancellationToken token) => club.PerfilAsync(socio, token);
}
