using Bomberos.Integration.Api.Clients;
using Bomberos.Integration.Api.Common;
using Bomberos.Integration.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bomberos.Integration.Api.Services;

/// <param name="Core">Respuesta de CORE; null si la operación se atendió con la copia local.</param>
public record OperacionResultado(CoreResult? Core, IActionResult Respuesta)
{
    public bool AplicadaEnCore => Core is { Exitoso: true };
}

public interface IOperacionService
{
    /// <summary>
    /// Envía la operación a CORE. Si CORE no responde, la valida con la copia local, la guarda como
    /// PENDIENTE y responde 202 Accepted.
    /// </summary>
    /// <param name="recursoAReservar">Recurso que debe estar DISPONIBLE en la copia local; queda RESERVADO.</param>
    /// <param name="coreCaido">Ya se sabe que CORE no responde: se encola sin volver a intentarlo.</param>
    Task<OperacionResultado> EjecutarOEncolarAsync(HttpContext ctx, string tipo, HttpMethod metodo, string endpoint,
        string? json, (string Tipo, int ExternalId)? recursoAReservar = null, string? bearer = null,
        string? referenciaProvisional = null, bool coreCaido = false);
}

public class OperacionService(ICoreClient core, IntegrationContext db) : IOperacionService
{
    public async Task<OperacionResultado> EjecutarOEncolarAsync(HttpContext ctx, string tipo, HttpMethod metodo,
        string endpoint, string? json, (string Tipo, int ExternalId)? recursoAReservar = null, string? bearer = null,
        string? referenciaProvisional = null, bool coreCaido = false)
    {
        var r = coreCaido
            ? new CoreResult(false, 0, null)
            : await core.SendAsync(metodo, endpoint, json, bearer ?? ctx.Bearer());
        if (r.Disponible)
            return new OperacionResultado(r, new ContentResult
            {
                StatusCode = r.StatusCode,
                Content = r.Body,
                ContentType = "application/json"
            });

        // ---- CORE caído: se atiende con la copia local ----
        ctx.MarcarAtendidoLocal();

        if (recursoAReservar is { } reserva)
        {
            var recurso = await db.CacheRecursos
                .FirstOrDefaultAsync(x => x.Tipo == reserva.Tipo && x.ExternalId == reserva.ExternalId);
            if (recurso is null || recurso.Estado != EstadosRecurso.Disponible)
            {
                var nombre = reserva.Tipo == TiposRecurso.Bombero ? "El bombero" : "El vehículo";
                return new OperacionResultado(null, new ConflictObjectResult(
                    Respuestas.Error($"{nombre} no está disponible según la copia local.")));
            }
            recurso.Estado = EstadosRecurso.Reservado;
            recurso.UltimaActualizacion = DateTime.UtcNow;
        }

        db.OperacionesPendientes.Add(new OperacionPendiente
        {
            FechaHora = DateTime.UtcNow,
            UsuarioId = ctx.User.FindFirst("sub")?.Value,
            Tipo = tipo,
            Metodo = metodo.Method,
            Endpoint = endpoint,
            Cuerpo = json,
            ReferenciaProvisional = referenciaProvisional,
            Estado = EstadosPendiente.Pendiente
        });
        await db.SaveChangesAsync();

        return new OperacionResultado(null, new AcceptedResult((string?)null, new
        {
            success = true,
            pendiente = true,
            referenciaProvisional,
            message = Respuestas.OperacionEncolada
        }));
    }
}
