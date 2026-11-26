using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Cinepolis.API;

public sealed class TokensClub
{
    private const string Emisor = "Cinepolis.API";
    private const string Audiencia = "Cinepolis.Club";
    private readonly SymmetricSecurityKey clave;

    public TokensClub(IConfiguration config, IHostEnvironment environment)
    {
        var secreto = config["Jwt:ClaveBase64"];
        var bytes = string.IsNullOrWhiteSpace(secreto) && environment.IsDevelopment()
            ? System.Security.Cryptography.RandomNumberGenerator.GetBytes(32)
            : Convert.FromBase64String(secreto ?? throw new InvalidOperationException("Configura Jwt:ClaveBase64."));
        if (bytes.Length < 32) throw new InvalidOperationException("La clave JWT requiere al menos 32 bytes.");
        clave = new(bytes);
    }

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

    public RespuestaLogin Emitir(int socio)
    {
        var vence = DateTime.UtcNow.AddHours(1);
        var jwt = new JwtSecurityToken(Emisor, Audiencia, [new Claim("sub", socio.ToString(System.Globalization.CultureInfo.InvariantCulture))],
            expires: vence, signingCredentials: new(clave, SecurityAlgorithms.HmacSha256));
        return new(new JwtSecurityTokenHandler().WriteToken(jwt), vence);
    }
}
public sealed record RespuestaLogin(string AccessToken, DateTime ExpiraUtc);

public sealed class CorreoOtp(IConfiguration config)
{
    public async Task EnviarAsync(string correo, string codigo, CancellationToken token)
    {
        var opciones = config.GetSection("Smtp");
        using var smtp = new SmtpClient(opciones["Host"] ?? throw new InvalidOperationException("Falta Smtp:Host."),
            opciones.GetValue("Puerto", 587)) { EnableSsl = opciones.GetValue("Tls", true) };
        if (opciones["Usuario"] is string usuario) smtp.Credentials = new NetworkCredential(usuario, opciones["Clave"]);
        using var mensaje = new MailMessage(opciones["De"] ?? throw new InvalidOperationException("Falta Smtp:De."),
            correo, "Código de acceso a Club Cinépolis", $"Tu código es {codigo}. Vence en cinco minutos.");
        await smtp.SendMailAsync(mensaje, token);
    }
}
