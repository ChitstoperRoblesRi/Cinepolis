using Cinepolis.Application;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;

namespace Cinepolis.Infrastructure;

/// <summary>Entrega el código de verificación al correo del socio.</summary>
public sealed class CorreoOtp(IConfiguration config) : ICorreoOtp
{
    /// <summary>Entrega el código de verificación al correo del socio.</summary>
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
