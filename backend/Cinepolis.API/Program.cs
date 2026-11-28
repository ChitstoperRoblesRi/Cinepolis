using Cinepolis.Domain;
using Cinepolis.API;
using Cinepolis.Application;
using Cinepolis.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ConflictosHandler>();
builder.Services.AddSingleton<TokensClub>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<TokensClub>((options, tokens) => tokens.Configurar(options));
builder.Services.AddAuthorization();
builder.Services.AddDataProtection();
builder.Services.AddSingleton<CredencialReserva>();
builder.Services.AddSingleton<ICorreoOtp, CorreoOtp>();
builder.Services.AddScoped<IAccesoClubRepository, ClubRepository>();
builder.Services.AddScoped<IClubQueryRepository, ClubRepository>();
builder.Services.AddScoped<AccesoClub>();
builder.Services.AddScoped<ConsultarClub>();
builder.Services.AddScoped<IOrdenRepository, OrdenRepository>();
builder.Services.AddScoped<IUnitOfWork, DapperUnitOfWork>();
builder.Services.AddSingleton<EstrategiaPrecioProductoBase>();
builder.Services.AddSingleton<IPrecioDulceriaStrategy>(sp => sp.GetRequiredService<EstrategiaPrecioProductoBase>());
builder.Services.AddSingleton<IPrecioDulceriaStrategy, EstrategiaSaboresPalomitasMaxExtra>();
builder.Services.AddSingleton<IPrecioDulceriaStrategy, EstrategiaPrecioCombo>();
builder.Services.AddSingleton<CalculadoraPrecioDulceriaContext>();
builder.Services.AddSingleton<ICotizadorDulceria>(sp => sp.GetRequiredService<CalculadoraPrecioDulceriaContext>());
builder.Services.AddSingleton<IValidadorSeleccionDulceria, ValidadorSeleccionDulceria>();
builder.Services.AddSingleton<IGeneradorQr, GeneradorQr>();
builder.Services.AddScoped<INotificadorVenta, NotificadorVenta>();
builder.Services.AddScoped<ServicioOrdenes>();
builder.Services.AddScoped<OrdenCompraBuilder>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();
var origins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? [];
builder.Services.AddCors(options => options.AddDefaultPolicy(policy =>
    policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod().AllowCredentials()));
builder.Services.AddSingleton<IDbConnectionFactory>(_ => new NpgsqlConnectionFactory(
    builder.Configuration.GetConnectionString("CinepolisDb") ?? throw new InvalidOperationException("Falta ConnectionStrings:CinepolisDb.")));
builder.Services.AddScoped<AsientoRepository>();
builder.Services.AddScoped<IFuncionAsientosRepository>(sp => sp.GetRequiredService<AsientoRepository>());
builder.Services.AddScoped<IReservaAsientosRepository>(sp => sp.GetRequiredService<AsientoRepository>());
builder.Services.AddScoped<IReservasVencidasRepository>(sp => sp.GetRequiredService<AsientoRepository>());
builder.Services.AddScoped<ConsultarMapaSala>();
builder.Services.AddHostedService<LimpiezaReservas>();
builder.Services.AddScoped<IComplejoQueryRepository, UbicacionesRepository>();
builder.Services.AddScoped<CarteleraRepository>();
builder.Services.AddScoped<IPeliculaQueryRepository>(sp => sp.GetRequiredService<CarteleraRepository>());
builder.Services.AddScoped<IFuncionesCarteleraRepository>(sp => sp.GetRequiredService<CarteleraRepository>());
builder.Services.AddSingleton<INormalizadorCatalogoHandler, NormalizadorCapacidadesHandler>();
builder.Services.AddSingleton<INormalizadorCatalogoHandler, NormalizadorTamanosEstandarHandler>();
builder.Services.AddSingleton<INormalizadorCatalogoHandler, NormalizadorCombosHandler>();
builder.Services.AddSingleton<INormalizadorCatalogoHandler, NormalizadorEtiquetasPasoHandler>();
builder.Services.AddSingleton<INormalizadorCatalogoHandler, NormalizadorLimitesSaboresHandler>();
builder.Services.AddSingleton<NormalizadorCatalogo>();
builder.Services.AddScoped<DulceriaRepository>();
builder.Services.AddScoped<IDulceriaCatalogoRepository>(sp => sp.GetRequiredService<DulceriaRepository>());
builder.Services.AddScoped<ConsultarUbicaciones>();
builder.Services.AddScoped<ConsultarCartelera>();
builder.Services.AddScoped<ConsultarDulceria>();
builder.Services.AddSingleton<INotificadorSala, NotificadorSala>();
builder.Services.AddHealthChecks().AddCheck<PostgresHealthCheck>("postgres", timeout: TimeSpan.FromSeconds(5));

var app = builder.Build();
app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.MapHub<SalaHub>("/hubs/sala");
app.MapControllers();
app.MapHealthChecks("/api/v1/health");
app.Run();

internal sealed class ConflictosHandler(IProblemDetailsService problemDetails) : IExceptionHandler
{
    /// <summary>Convierte conflictos de compra en respuestas HTTP de conflicto.</summary>
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception error, CancellationToken token)
    {
        if (error is not ConflictoCompra) return false;
        context.Response.StatusCode = StatusCodes.Status409Conflict;
        return await problemDetails.TryWriteAsync(new() { HttpContext = context,
            ProblemDetails = new() { Status = 409, Title = error.Message } });
    }
}
