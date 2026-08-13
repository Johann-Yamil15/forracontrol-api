using System.Text.Json.Serialization;
using ForraControl.API.Common;
using ForraControl.API.Data;
using ForraControl.API.Interfaces;
using ForraControl.API.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

// Railway (y otros hosts) asignan el puerto dinámicamente via la variable de entorno PORT.
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

// ── JSON: camelCase (default) + ignorar nulls, igual que el API viejo ──────
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ── Base de datos (PostgreSQL) ──────────────────────────────────────────
var connectionString = builder.Configuration.GetConnectionString("Database");

Console.WriteLine(string.IsNullOrEmpty(connectionString)
    ? "[Startup] ConnectionStrings:Database está VACÍA — revisa la variable de entorno ConnectionStrings__Database en Railway."
    : $"[Startup] ConnectionStrings:Database configurada (longitud={connectionString.Length}).");

if (string.IsNullOrEmpty(connectionString))
{
    var relacionadas = Environment.GetEnvironmentVariables().Keys.Cast<string>()
        .Where(k => k.Contains("Connection", StringComparison.OrdinalIgnoreCase))
        .ToList();
    Console.WriteLine(relacionadas.Count > 0
        ? $"[Startup] Variables de entorno relacionadas encontradas: {string.Join(", ", relacionadas)}"
        : "[Startup] No se encontró ninguna variable de entorno que contenga 'Connection'. La variable no está llegando al contenedor.");

    throw new InvalidOperationException(
        "ConnectionStrings:Database no está configurada. Define la variable de entorno " +
        "ConnectionStrings__Database (doble guion bajo) en el servicio de Railway que corre esta imagen.");
}

builder.Services.AddDbContext<ForraDbContext>(options =>
    options.UseNpgsql(connectionString)
           .UseSnakeCaseNamingConvention());

// ── Inyección de dependencias ───────────────────────────────────────────
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IProductoService, ProductoService>();
builder.Services.AddScoped<IClienteService, ClienteService>();
builder.Services.AddScoped<IVentaService, VentaService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IConfigService, ConfigService>();

// ── CORS para Flutter (igual que el API viejo: abierto) ────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy => policy
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader());
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowAll");

// ── Imágenes subidas (productos) ────────────────────────────────────────
// En Railway, Uploads__Path debe apuntar a un Volume montado (ej. /data/uploads)
// para que las imágenes sobrevivan reinicios/redeploys — sin eso, el disco del
// contenedor es efímero y se pierden. Si la carpeta no se puede crear/escribir
// (permisos del Volume, disco lleno, etc.) se loguea y se sigue arrancando —
// un problema de subida de imágenes no debe tumbar el resto de la API.
var uploadsRoot = UploadPaths.GetRoot(app.Configuration);
try
{
    Directory.CreateDirectory(Path.Combine(uploadsRoot, "productos"));
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(uploadsRoot),
        RequestPath = "/uploads",
    });
    Console.WriteLine($"[Startup] Carpeta de uploads lista en: {uploadsRoot}");
}
catch (Exception ex)
{
    Console.WriteLine($"[Startup] No se pudo preparar la carpeta de uploads ({uploadsRoot}): {ex.Message}. " +
        "La subida de imágenes de productos no funcionará hasta que se resuelva, pero el resto de la API sigue disponible.");
}

app.UseAuthorization();
app.MapControllers();

// Aplica migraciones pendientes y siembra datos iniciales al arrancar.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ForraDbContext>();
    await db.Database.MigrateAsync();
    await DbInitializer.SeedAsync(db);
}

app.Run();
