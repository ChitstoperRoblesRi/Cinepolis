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
builder.Services.AddSingleton<CorreoOtp>();
builder.Services.AddScoped<IClubRepository, ClubRepository>();
builder.Services.AddScoped<IOrdenRepository, OrdenRepository>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();
var origins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? [];
builder.Services.AddCors(options => options.AddDefaultPolicy(policy =>
    policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod().AllowCredentials()));
builder.Services.AddSingleton(_ => new NpgsqlConnectionFactory(
    builder.Configuration.GetConnectionString("CinepolisDb") ?? throw new InvalidOperationException("Falta ConnectionStrings:Postgres.")));
builder.Services.AddScoped<IAsientoRepository, AsientoRepository>();
builder.Services.AddScoped<ConsultarMapaSala>();
builder.Services.AddHostedService<LimpiezaReservas>();
builder.Services.AddScoped<IUbicacionesRepository, UbicacionesRepository>();
builder.Services.AddScoped<ICarteleraRepository, CarteleraRepository>();
builder.Services.AddScoped<IDulceriaRepository, DulceriaRepository>();
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
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception error, CancellationToken token)
    {
        if (error is not ConflictoCompra) return false;
        context.Response.StatusCode = StatusCodes.Status409Conflict;
        return await problemDetails.TryWriteAsync(new() { HttpContext = context,
            ProblemDetails = new() { Status = 409, Title = error.Message } });
    }
}
