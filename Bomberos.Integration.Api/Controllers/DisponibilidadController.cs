using System.Text.Json;
using Bomberos.Integration.Api.Clients;
using Bomberos.Integration.Api.Common;
using Bomberos.Integration.Api.Data;
using Bomberos.Integration.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bomberos.Integration.Api.Controllers;

public record RecursoDisponible(int Id, string Descripcion, string Estado);

/// <summary>
/// Disponibilidad de bomberos y vehículos. Responde con los datos de CORE o, si está caído, con la copia local
/// (cabecera X-Atendido-Local: true). El formato es el mismo en ambos casos.
/// </summary>
[ApiController]
[Route("api/v1")]
[Authorize(Policy = "Personal")]
public class DisponibilidadController(ICoreClient core, IntegrationContext db) : ControllerBase
{
    [HttpGet("bomberos/disponibles")]
    public Task<ActionResult<List<RecursoDisponible>>> Bomberos()
        => ConsultarAsync("api/v1/bomberos/disponibles", TiposRecurso.Bombero, RecursoCache.DesdeBombero);

    [HttpGet("vehiculos/disponibles")]
    public Task<ActionResult<List<RecursoDisponible>>> Vehiculos()
        => ConsultarAsync("api/v1/vehiculos/disponibles", TiposRecurso.Vehiculo, RecursoCache.DesdeVehiculo);

    private async Task<ActionResult<List<RecursoDisponible>>> ConsultarAsync(string endpoint, string tipo,
        Func<JsonElement, CacheRecurso> convertir)
    {
        var r = await core.SendAsync(HttpMethod.Get, endpoint, null, HttpContext.Bearer());
        if (r.Disponible)
        {
            if (!r.Exitoso)
                return new ContentResult { StatusCode = r.StatusCode, Content = r.Body, ContentType = "application/json" };

            using var doc = JsonDocument.Parse(r.Body!);
            return doc.RootElement.EnumerateArray()
                .Select(convertir)
                .Select(c => new RecursoDisponible(c.ExternalId, c.Descripcion, EstadosRecurso.Disponible))
                .ToList();
        }

        // ---- CORE caído: copia local ----
        HttpContext.MarcarAtendidoLocal();
        return await db.CacheRecursos.AsNoTracking()
            .Where(c => c.Tipo == tipo && c.Estado == EstadosRecurso.Disponible)
            .OrderBy(c => c.Descripcion)
            .Select(c => new RecursoDisponible(c.ExternalId, c.Descripcion, c.Estado))
            .ToListAsync();
    }
}

/// <summary>Consultas de administración sobre los datos propios de Integración.</summary>
[ApiController]
[Route("api/v1/integracion")]
[Authorize(Roles = "ADMINISTRADOR")]
public class IntegracionController(IntegrationContext db) : ControllerBase
{
    /// <summary>Operaciones recibidas con CORE caído. Filtro opcional: estado (PENDIENTE, APLICADA, RECHAZADA).</summary>
    [HttpGet("pendientes")]
    public async Task<ActionResult<List<OperacionPendiente>>> Pendientes([FromQuery] string? estado)
    {
        var query = db.OperacionesPendientes.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(estado)) query = query.Where(p => p.Estado == estado.Trim().ToUpper());
        return await query.OrderByDescending(p => p.Id).Take(500).ToListAsync();
    }

    [HttpGet("logs")]
    public async Task<ActionResult<List<IntegrationLog>>> Logs([FromQuery] int limite = 200, [FromQuery] bool soloLocal = false)
    {
        var query = db.IntegrationLogs.AsNoTracking();
        if (soloLocal) query = query.Where(l => l.AtendidoLocal);
        return await query.OrderByDescending(l => l.Id).Take(Math.Clamp(limite, 1, 1000)).ToListAsync();
    }

    [HttpGet("cache")]
    public async Task<ActionResult<List<CacheRecurso>>> Cache()
        => await db.CacheRecursos.AsNoTracking().OrderBy(c => c.Tipo).ThenBy(c => c.Descripcion).ToListAsync();
}
