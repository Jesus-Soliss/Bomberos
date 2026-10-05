using System.Text.Json;
using Bomberos.Integration.Api.Clients;
using Bomberos.Integration.Api.Common;
using Bomberos.Integration.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Bomberos.Integration.Api.Services;

/// <param name="CoreDisponible">false si CORE no respondió.</param>
/// <param name="Token">null si CORE está caído o rechazó las credenciales de la cuenta de servicio.</param>
public record LoginServicio(bool CoreDisponible, string? Token);

public interface ISyncService
{
    /// <param name="forzar">Ignora el token guardado y vuelve a iniciar sesión (sirve para comprobar si CORE responde).</param>
    Task<LoginServicio> LoginServicioAsync(bool forzar = false);
    Task AplicarPendientesAsync(string token);
    /// <summary>CORE sigue sin responder: cuenta un reintento para cada operación pendiente.</summary>
    Task RegistrarCoreCaidoAsync();
    Task RefrescarCacheAsync(string token);
    /// <summary>Actualiza un solo recurso en la copia local después de una operación exitosa.</summary>
    Task RefrescarRecursoAsync(string tipo, int externalId, string? bearer);
}

/// <summary>Token de la cuenta de servicio, compartido entre peticiones.</summary>
public class ServiceTokenCache
{
    public string? Token { get; set; }
    public DateTime Expira { get; set; }
    public bool Vigente => Token is not null && Expira > DateTime.UtcNow.AddMinutes(2);
}

public class SyncService(
    ICoreClient core,
    IntegrationContext db,
    ServiceTokenCache tokenCache,
    IOptions<CoreOptions> coreOptions,
    IOptions<SyncOptions> syncOptions,
    ILogger<SyncService> logger) : ISyncService
{
    public async Task<LoginServicio> LoginServicioAsync(bool forzar = false)
    {
        if (!forzar && tokenCache.Vigente) return new LoginServicio(true, tokenCache.Token);

        var json = JsonSerializer.Serialize(new
        {
            username = coreOptions.Value.ServiceUser,
            password = coreOptions.Value.ServicePassword
        });
        var r = await core.SendAsync(HttpMethod.Post, "api/v1/auth/login", json, null);
        if (!r.Disponible) return new LoginServicio(false, null);
        if (!r.Exitoso)
        {
            logger.LogError("CORE rechazó el inicio de sesión de la cuenta de servicio ({Status}). Revise Core:ServiceUser y Core:ServicePassword.", r.StatusCode);
            return new LoginServicio(true, null);
        }

        using var doc = JsonDocument.Parse(r.Body!);
        tokenCache.Token = doc.RootElement.GetProperty("token").GetString();
        tokenCache.Expira = doc.RootElement.TryGetProperty("expira", out var expira) && expira.TryGetDateTime(out var fecha)
            ? DateTime.SpecifyKind(fecha, DateTimeKind.Utc)
            : DateTime.UtcNow.AddMinutes(30);
        return new LoginServicio(true, tokenCache.Token);
    }

    public async Task AplicarPendientesAsync(string token)
    {
        var pendientes = await db.OperacionesPendientes
            .Where(p => p.Estado == EstadosPendiente.Pendiente).OrderBy(p => p.Id).ToListAsync();

        foreach (var p in pendientes)
        {
            var r = await core.SendAsync(new HttpMethod(p.Metodo), p.Endpoint, p.Cuerpo, token);
            if (!r.Disponible)                      // CORE volvió a caer: detener y reintentar luego
            {
                ContarReintento(p);
                await db.SaveChangesAsync();
                break;
            }

            if (r.Exitoso)
            {
                p.Estado = EstadosPendiente.Aplicada;
                p.FechaAplicacion = DateTime.UtcNow;
            }
            else
            {
                // CORE aplicó sus reglas y la rechazó.
                p.Estado = EstadosPendiente.Rechazada;
                p.Motivo = Recortar(ExtraerMensaje(r.Body) ?? $"CORE respondió {r.StatusCode}.", 500);
            }
            await db.SaveChangesAsync();
            logger.LogInformation("Operación pendiente {Id} ({Tipo}): {Estado}", p.Id, p.Tipo, p.Estado);
        }
    }

    public async Task RegistrarCoreCaidoAsync()
    {
        var pendientes = await db.OperacionesPendientes
            .Where(p => p.Estado == EstadosPendiente.Pendiente).ToListAsync();
        if (pendientes.Count == 0) return;

        foreach (var p in pendientes) ContarReintento(p);
        await db.SaveChangesAsync();
    }

    public async Task RefrescarCacheAsync(string token)
    {
        var bomberos = await core.SendAsync(HttpMethod.Get, "api/v1/bomberos", null, token);
        var vehiculos = await core.SendAsync(HttpMethod.Get, "api/v1/vehiculos", null, token);
        if (!bomberos.Exitoso || !vehiculos.Exitoso) return;

        var cache = await db.CacheRecursos.ToListAsync();
        using (var doc = JsonDocument.Parse(bomberos.Body!))
            foreach (var b in doc.RootElement.EnumerateArray()) Upsert(cache, RecursoCache.DesdeBombero(b));
        using (var doc = JsonDocument.Parse(vehiculos.Body!))
            foreach (var v in doc.RootElement.EnumerateArray()) Upsert(cache, RecursoCache.DesdeVehiculo(v));

        await db.SaveChangesAsync();
    }

    public async Task RefrescarRecursoAsync(string tipo, int externalId, string? bearer)
    {
        try
        {
            var ruta = tipo == TiposRecurso.Bombero ? "bomberos" : "vehiculos";
            var r = await core.SendAsync(HttpMethod.Get, $"api/v1/{ruta}/{externalId}", null, bearer);
            if (!r.Exitoso) return;

            using var doc = JsonDocument.Parse(r.Body!);
            var recurso = tipo == TiposRecurso.Bombero
                ? RecursoCache.DesdeBombero(doc.RootElement)
                : RecursoCache.DesdeVehiculo(doc.RootElement);
            var cache = await db.CacheRecursos.Where(c => c.Tipo == tipo && c.ExternalId == externalId).ToListAsync();
            Upsert(cache, recurso);
            await db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // La copia local se corrige en el siguiente refresco periódico.
            logger.LogWarning(ex, "No se pudo refrescar {Tipo} {Id} en la copia local", tipo, externalId);
        }
    }

    private void Upsert(List<CacheRecurso> cache, CacheRecurso nuevo)
    {
        var actual = cache.FirstOrDefault(c => c.Tipo == nuevo.Tipo && c.ExternalId == nuevo.ExternalId);
        if (actual is null)
        {
            db.CacheRecursos.Add(nuevo);
            cache.Add(nuevo);
            return;
        }
        actual.Descripcion = nuevo.Descripcion;
        actual.Estado = nuevo.Estado;
        actual.UltimaActualizacion = nuevo.UltimaActualizacion;
    }

    private void ContarReintento(OperacionPendiente p)
    {
        p.Reintentos++;
        if (p.Reintentos < syncOptions.Value.MaxReintentos) return;
        p.Estado = EstadosPendiente.Rechazada;
        p.Motivo = "Se superó el máximo de reintentos.";
    }

    private static string? ExtraerMensaje(string? body)
    {
        if (string.IsNullOrWhiteSpace(body)) return null;
        try
        {
            using var doc = JsonDocument.Parse(body);
            return doc.RootElement.ValueKind == JsonValueKind.Object
                   && doc.RootElement.TryGetProperty("message", out var m) ? m.GetString() : body;
        }
        catch (JsonException)
        {
            return body;
        }
    }

    private static string Recortar(string texto, int longitud) => texto.Length > longitud ? texto[..longitud] : texto;
}

/// <summary>Traduce la respuesta de CORE (bomberos y vehículos) a filas de CacheRecurso.</summary>
public static class RecursoCache
{
    public static CacheRecurso DesdeBombero(JsonElement b)
    {
        var activo = b.GetProperty("estado").GetString() == "ACTIVO";
        var enServicio = b.TryGetProperty("enServicio", out var s) && s.GetBoolean();
        return new CacheRecurso
        {
            Tipo = TiposRecurso.Bombero,
            ExternalId = b.GetProperty("id").GetInt32(),
            Descripcion = Recortar($"{b.GetProperty("nombre").GetString()} {b.GetProperty("apellido").GetString()} ({b.GetProperty("rango").GetString()})"),
            Estado = !activo ? EstadosRecurso.NoDisponible : enServicio ? EstadosRecurso.EnServicio : EstadosRecurso.Disponible,
            UltimaActualizacion = DateTime.UtcNow
        };
    }

    public static CacheRecurso DesdeVehiculo(JsonElement v)
    {
        var estado = v.GetProperty("estado").GetString();
        return new CacheRecurso
        {
            Tipo = TiposRecurso.Vehiculo,
            ExternalId = v.GetProperty("id").GetInt32(),
            Descripcion = Recortar($"{v.GetProperty("placa").GetString()} ({v.GetProperty("tipo").GetString()})"),
            Estado = estado is EstadosRecurso.Disponible or EstadosRecurso.EnServicio ? estado : EstadosRecurso.NoDisponible,
            UltimaActualizacion = DateTime.UtcNow
        };
    }

    private static string Recortar(string texto) => texto.Length > 150 ? texto[..150] : texto;
}
