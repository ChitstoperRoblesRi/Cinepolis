using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Cinepolis.API;

/// <summary>Configura la firma y la validación de los tokens del programa Club.</summary>
public sealed class TokensClub
{
    private const string Emisor = "Cinepolis.API";
    private const string Audiencia = "Cinepolis.Club";
    private readonly SymmetricSecurityKey clave;

    /// <summary>Configura la firma y la validación de los tokens del programa Club.</summary>
    public TokensClub(IConfiguration config, IHostEnvironment environment)
    {
        var secreto = config["Jwt:ClaveBase64"];
        var bytes = string.IsNullOrWhiteSpace(secreto) && environment.IsDevelopment()
            ? System.Security.Cryptography.RandomNumberGenerator.GetBytes(32)
            : Convert.FromBase64String(secreto ?? throw new InvalidOperationException("Configura Jwt:ClaveBase64."));
        if (bytes.Length < 32) throw new InvalidOperationException("La clave JWT requiere al menos 32 bytes.");
        clave = new(bytes);
    }

    /// <summary>Define los criterios de validación de tokens de acceso.</summary>
    public void Configurar(JwtBearerOptions opciones)
    {
        opciones.MapInboundClaims = false;
        opciones.TokenValidationParameters = new()
        {
            ValidateIssuer = true, ValidIssuer = Emisor, ValidateAudience = true, ValidAudience = Audiencia,
            ValidateIssuerSigningKey = true, IssuerSigningKey = clave, ValidateLifetime = true,
            ValidAlgorithms = [SecurityAlgorithms.HmacSha256], ClockSkew = TimeSpan.FromSeconds(15)
        };
    }

    /// <summary>Emite una credencial temporal para el titular.</summary>
    public RespuestaLogin Emitir(int socio)
    {
        var vence = DateTime.UtcNow.AddHours(1);
        var jwt = new JwtSecurityToken(Emisor, Audiencia, [new Claim("sub", socio.ToString(System.Globalization.CultureInfo.InvariantCulture))],
            expires: vence, signingCredentials: new(clave, SecurityAlgorithms.HmacSha256));
        return new(new JwtSecurityTokenHandler().WriteToken(jwt), vence);
    }
}
/// <summary>Token de acceso del socio y fecha de expiración UTC.</summary>
public sealed record RespuestaLogin(string AccessToken, DateTime ExpiraUtc);
