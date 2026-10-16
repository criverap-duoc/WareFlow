using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;

// Referencias a nuestros proyectos
using WareFlow.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using WareFlow.Core.Models;
using WareFlow.API.Data;
using WareFlow.API.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configuración de Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "WareFlow API", 
        Version = "v1",
        Description = "API para gestión de inventarios y órdenes"
    });
    
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingrese el token JWT con el formato: Bearer {token}"
    });
    
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Base de datos
// EnableRetryOnFailure: Azure SQL Serverless entra en auto-pause y la primera
// conexión falla con error transitorio (40613). Con 5 reintentos y hasta 30s de
// espera entre ellos, la app sobrevive al despertar de la base de datos.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null)));

// Identity
builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// JWT
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.ASCII.GetBytes(jwtSettings["Key"] ?? throw new InvalidOperationException("JWT Key not configured"));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };

    // El DTE se abre con window.open (pestaña nueva), que no puede enviar el
    // header Authorization: se acepta el token también por query string
    // (?access_token=...) para ese caso puntual.
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            if (!string.IsNullOrEmpty(accessToken))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

// Fase B.3a: emisión y rotación de refresh tokens
builder.Services.AddScoped<RefreshTokenService>();

// Fase IA.1: predicción de demanda (promedio de ventas de los últimos 30 días)
builder.Services.AddScoped<DemandForecastService>();

// CORS
// Los orígenes permitidos se leen de configuración (Cors:AllowedOrigins), lo que
// permite apuntar a Vercel/Azure en producción vía appsettings.Production.json o
// variables de entorno (Cors__AllowedOrigins__0, __1, ...). Fallback: dev local.
var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>()
    ?? new[] { "http://localhost:5173", "http://localhost:3000", "http://localhost:4200" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()
              .WithExposedHeaders("Content-Disposition");
    });
});

var app = builder.Build();

// Swagger habilitado en Development y en Production (contenedor Docker, proyecto de portafolio)
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => 
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "WareFlow API v1");
        c.RoutePrefix = "swagger";
    });
}

// Solo redirigir a HTTPS si no estamos en contenedor (Docker usa HTTP interno)
var isRunningInContainer = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";
if (!app.Environment.IsDevelopment() && !isRunningInContainer)
{
    app.UseHttpsRedirection();
}

app.UseCors("AllowReactApp");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Aplicar migraciones pendientes al arrancar (crea la base de datos si no existe).
// EnsureCreated() ya no se usa: crea el esquema a partir del modelo actual e ignora
// las migraciones, por lo que una base recién creada (clon del repo, volumen Docker
// nuevo) quedaría sin tablas posteriores como RefreshTokens.
// Migrate() es una API relacional: los tests de integración usan EF Core InMemory
// (su esquema lo crea CustomWebApplicationFactory con EnsureCreated).
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    if (dbContext.Database.IsRelational())
    {
        try
        {
            dbContext.Database.Migrate();
        }
        catch (Exception ex)
        {
            // La DB dormida (Azure SQL Serverless, auto-pause) no debe tumbar el
            // proceso: la app arranca igual, /Health responde 200 y los endpoints
            // que usan DB reintentan vía EnableRetryOnFailure del DbContext.
            logger.LogError(ex, "Error al aplicar migraciones al arrancar. La app seguirá corriendo y reintentará en el primer request.");
        }
    }
}

// Fase B.1: roles de usuario. Deben existir antes de asignar roles: la tabla
// AspNetRoles ya fue creada por Migrate en el bloque anterior. El seeder
// serializa la creación para evitar roles duplicados cuando arrancan varios
// hosts en paralelo (tests de integración y réplicas en Docker).
using (var scope = app.Services.CreateScope())
{
    // Repara en el arranque a los usuarios sin rol (creados antes de la Fase B.1):
    // el más antiguo recibe Admin y el resto Vendedor.
    var seededRoles = await RoleSeeder.SeedAsync(scope.ServiceProvider);
    if (seededRoles > 0)
        app.Logger.LogInformation(
            "RoleSeeder: {Count} usuario(s) sin rol recibieron rol automáticamente",
            seededRoles);
}

app.Run();

// Expuesto para que WebApplicationFactory<Program> pueda referenciar el ensamblado
// de la API desde el proyecto de pruebas de integración (requerido por minimal hosting).
public partial class Program { }
