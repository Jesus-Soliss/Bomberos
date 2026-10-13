using System.Text;
using Bomberos.Integration.Api.BackgroundServices;
using Bomberos.Integration.Api.Clients;
using Bomberos.Integration.Api.Common;
using Bomberos.Integration.Api.Data;
using Bomberos.Integration.Api.Filters;
using Bomberos.Integration.Api.Middleware;
using Bomberos.Integration.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

// Nombre de la ventana de consola, para distinguirla al ejecutar los cuatro proyectos a la vez.
try { Console.Title = "Bomberos - Integration.Api"; } catch (IOException) { /* sin consola */ }

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

builder.Services.Configure<CoreOptions>(config.GetSection("Core"));
builder.Services.Configure<Centro911Options>(config.GetSection("Centro911"));
builder.Services.Configure<SyncOptions>(config.GetSection("Sync"));

var coreOptions = config.GetSection("Core").Get<CoreOptions>() ?? new CoreOptions();
var centro911 = config.GetSection("Centro911").Get<Centro911Options>() ?? new Centro911Options();
var jwt = config.GetSection("JwtSettings").Get<JwtOptions>() ?? new JwtOptions();
if (jwt.Key.Length < 32)
    throw new InvalidOperationException(
        "JwtSettings:Key debe tener al menos 32 caracteres y ser la misma clave que usa la WebApi.");

builder.Services.AddDbContext<IntegrationContext>(o => o.UseSqlServer(config.GetConnectionString("DefaultConnection")));

// Timeout corto para detectar rápido la caída de CORE.
builder.Services.AddHttpClient<ICoreClient, CoreClient>(c =>
{
    c.BaseAddress = new Uri(coreOptions.BaseUrl);
    c.Timeout = TimeSpan.FromSeconds(coreOptions.TimeoutSegundos);
});
builder.Services.AddHttpClient<ICliente911, Cliente911>(c =>
{
    c.BaseAddress = new Uri(centro911.BaseUrl);
    c.Timeout = TimeSpan.FromSeconds(5);
    c.DefaultRequestHeaders.Add(ApiKeyFilter.Cabecera, centro911.ApiKey);
});

builder.Services.AddSingleton<ServiceTokenCache>();
builder.Services.AddScoped<ISyncService, SyncService>();
builder.Services.AddScoped<IOperacionService, OperacionService>();
builder.Services.AddScoped<ApiKeyFilter>();
builder.Services.AddHostedService<SincronizacionService>();

// Integración valida el JWT con la misma clave que la WebApi, para reconocer al usuario aunque CORE esté caído.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(o =>
{
    o.MapInboundClaims = false;
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true, ValidateAudience = true,
        ValidateLifetime = true, ValidateIssuerSigningKey = true,
        ValidIssuer = jwt.Issuer, ValidAudience = jwt.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
        NameClaimType = "name",
        RoleClaimType = "role",
        ClockSkew = TimeSpan.FromSeconds(30)
    };
    o.Events = new JwtBearerEvents
    {
        OnChallenge = async ctx =>
        {
            ctx.HandleResponse();
            ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await ctx.Response.WriteAsJsonAsync(Respuestas.Error("No autenticado o token inválido."));
        },
        OnForbidden = async ctx =>
        {
            ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
            await ctx.Response.WriteAsJsonAsync(Respuestas.Error("No tiene permiso para realizar esta acción."));
        }
    };
});
// Personal del sistema: cualquier usuario autenticado que no sea CLIENTE.
builder.Services.AddAuthorization(o => o.AddPolicy("Personal",
    p => p.RequireAuthenticatedUser().RequireAssertion(c => !c.User.IsInRole("CLIENTE"))));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Bomberos - Integration.Api",
        Version = "v1",
        Description = "Enlace entre CORE, el canal WEB y el Centro 911. Además de los endpoints listados, cualquier otra "
                      + "ruta bajo /api/v1 (usuarios, roles, estaciones, bomberos, vehículos, emergencias, dashboard, "
                      + "auditoría) se reenvía a CORE sin cambios."
    });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Token JWT obtenido en /api/v1/auth/login (sin el prefijo Bearer)."
    });
    c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Header,
        Name = ApiKeyFilter.Cabecera,
        Description = "Clave del Centro 911 (solo para /integration/911)."
    });
    c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
    c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("ApiKey", document)] = []
    });
});

var app = builder.Build();

app.UseMiddleware<IntegrationLogMiddleware>();

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
    await scope.ServiceProvider.GetRequiredService<IntegrationContext>().Database.MigrateAsync();

app.Run();
