using System.Text.Json;
using Bomberos.Integration.Api.Clients;
using Bomberos.Integration.Api.Common;
using Bomberos.Integration.Api.Data;
using Bomberos.Integration.Api.Filters;
using Bomberos.Integration.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bomberos.Integration.Api.Controllers;

public record Reporte911(string? Referencia, string? Tipo, string? Descripcion, string? Direccion, string? Prioridad);

/// <summary>Servicios publicados para el Centro 911. Se autentican con la cabecera X-Api-Key, no con JWT.</summary>
[ApiController]
[Route("integration/911/emergencias")]
[AllowAnonymous]
[ServiceFilter(typeof(ApiKeyFilter))]
public class Centro911Controller(
    ICoreClient core,
    ISyncService sync,
    IOperacionService operaciones,
    IntegrationContext db) : ControllerBase
{
    private const string Endpoint = "api/v1/emergencias/externa";

    /// <summary>
    /// Recibe un reporte de emergencia. 201 si CORE la registró (PENDIENTE), 409 si la referencia ya existe,
    /// 202 con una referencia provisional si CORE está caído.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Reportar([FromBody] Reporte911 reporte)
    {
        var faltantes = new[]
        {
            (nameof(reporte.Referencia), reporte.Referencia), (nameof(reporte.Tipo), reporte.Tipo),
            (nameof(reporte.Descripcion), reporte.Descripcion), (nameof(reporte.Direccion), reporte.Direccion),
            (nameof(reporte.Prioridad), reporte.Prioridad)
        }.Where(c => string.IsNullOrWhiteSpace(c.Item2)).Select(c => c.Item1.ToLowerInvariant()).ToList();
        if (faltantes.Count > 0)
            return BadRequest(Respuestas.Error($"Faltan campos requeridos: {string.Join(", ", faltantes)}."));

        var json = JsonSerializer.Serialize(reporte, JsonSerializerOptions.Web);
        var login = await sync.LoginServicioAsync();     // cuenta de servicio
        if (login.CoreDisponible && login.Token is null)
            return StatusCode(StatusCodes.Status502BadGateway, Respuestas.Error("No se pudo autenticar ante el sistema central."));

        var provisional = EmergenciasController.NuevaReferencia();
        var r = await operaciones.EjecutarOEncolarAsync(HttpContext, TiposOperacion.Reporte911, HttpMethod.Post,
            Endpoint, json, bearer: login.Token, referenciaProvisional: provisional, coreCaido: !login.CoreDisponible);

        if (r.Core is null)   // CORE caído: quedó pendiente
            return Accepted(new
            {
                referencia = reporte.Referencia,
                referenciaProvisional = provisional,
                estado = "PENDIENTE_DE_REGISTRO",
                message = Respuestas.OperacionEncolada
            });

        return r.Core.StatusCode switch
        {
            StatusCodes.Status201Created => StatusCode(StatusCodes.Status201Created, new { referencia = reporte.Referencia, estado = "PENDIENTE" }),
            StatusCodes.Status409Conflict => Conflict(Respuestas.Error("Ya existe una emergencia registrada con esa referencia.")),
            _ => r.Respuesta
        };
    }

    /// <summary>Consulta el estado de una emergencia por su referencia (o por la referencia provisional).</summary>
    [HttpGet("{referencia}")]
    public async Task<IActionResult> ConsultarEstado(string referencia)
    {
        var login = await sync.LoginServicioAsync();
        if (login.Token is not null)
        {
            var r = await core.SendAsync(HttpMethod.Get, $"{Endpoint}/{Uri.EscapeDataString(referencia)}", null, login.Token);
            if (r.Exitoso)
            {
                using var doc = JsonDocument.Parse(r.Body!);
                return Ok(new
                {
                    referencia = doc.RootElement.GetProperty("referenciaExterna").GetString(),
                    estado = doc.RootElement.GetProperty("estado").GetString(),
                    fechaHora = doc.RootElement.GetProperty("fechaHora").GetDateTime()
                });
            }
            if (r.Disponible && r.StatusCode != StatusCodes.Status404NotFound)
                return new ContentResult { StatusCode = r.StatusCode, Content = r.Body, ContentType = "application/json" };
        }

        // No está en CORE (o CORE no responde): puede ser un reporte que sigue en cola.
        var pendiente = await db.OperacionesPendientes.AsNoTracking()
            .Where(p => p.Tipo == TiposOperacion.Reporte911 && p.ReferenciaProvisional == referencia)
            .OrderByDescending(p => p.Id).FirstOrDefaultAsync();
        if (pendiente is not null)
        {
            HttpContext.MarcarAtendidoLocal();
            return Ok(new
            {
                referencia,
                estado = pendiente.Estado == EstadosPendiente.Pendiente ? "PENDIENTE_DE_REGISTRO" : pendiente.Estado,
                motivo = pendiente.Motivo,
                fechaHora = pendiente.FechaHora
            });
        }

        return login.CoreDisponible
            ? NotFound(Respuestas.Error("No existe una emergencia con esa referencia."))
            : StatusCode(StatusCodes.Status503ServiceUnavailable, Respuestas.Error(Respuestas.CoreNoDisponible));
    }
}
