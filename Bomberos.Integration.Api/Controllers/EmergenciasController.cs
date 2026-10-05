using System.Text.Json;
using Bomberos.Integration.Api.Clients;
using Bomberos.Integration.Api.Common;
using Bomberos.Integration.Api.Data;
using Bomberos.Integration.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bomberos.Integration.Api.Controllers;

/// <summary>
/// Endpoints con lógica propia: siguen funcionando con la copia local y la cola de pendientes si CORE cae.
/// Las reglas de negocio las aplica siempre CORE; aquí solo hay validaciones básicas.
/// </summary>
[ApiController]
[Route("api/v1/emergencias")]
[Authorize(Roles = "ADMINISTRADOR,OPERADOR")]
public class EmergenciasController(
    IOperacionService operaciones,
    ISyncService sync,
    ICoreClient core,
    ICliente911 cliente911) : ControllerBase
{
    /// <summary>Registra una emergencia. Con CORE caído responde 202 y queda pendiente.</summary>
    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] JsonElement emergencia)
    {
        // Solo se reenvían (o encolan) los cuatro campos conocidos, ya validados y normalizados.
        string? tipo = Texto(emergencia, "tipo"), prioridad = Texto(emergencia, "prioridad"),
            descripcion = Texto(emergencia, "descripcion"), direccion = Texto(emergencia, "direccion");
        var errores = ValidadorEmergencia.Validar(tipo, prioridad, descripcion, direccion);
        if (errores.Count > 0)
            return BadRequest(new { success = false, message = string.Join(" ", errores), errors = errores });

        var json = JsonSerializer.Serialize(new
        {
            tipo = tipo!.Trim().ToUpperInvariant(),
            descripcion = descripcion!.Trim(),
            direccion = direccion!.Trim(),
            prioridad = prioridad!.Trim().ToUpperInvariant()
        });
        var r = await operaciones.EjecutarOEncolarAsync(HttpContext, TiposOperacion.CrearEmergencia, HttpMethod.Post,
            "api/v1/emergencias", json, referenciaProvisional: NuevaReferencia());
        return r.Respuesta;
    }

    /// <summary>
    /// Cambia el estado de una emergencia (requiere CORE). Si la emergencia vino del Centro 911, le avisa el cambio.
    /// </summary>
    [HttpPatch("{id:int}/estado")]
    public async Task<IActionResult> CambiarEstado(int id, [FromBody] JsonElement cambio)
    {
        var r = await core.SendAsync(HttpMethod.Patch, $"api/v1/emergencias/{id}/estado", cambio.GetRawText(), HttpContext.Bearer());
        if (!r.Disponible)
            return StatusCode(StatusCodes.Status503ServiceUnavailable, Respuestas.Error(Respuestas.CoreNoDisponible));

        if (r.Exitoso)
        {
            await AvisarAl911Async(r.Body);
            // Al finalizar una emergencia CORE libera sus recursos: la copia local debe reflejarlo.
            if (cambio.ValueKind == JsonValueKind.Object && cambio.TryGetProperty("estado", out var estado)
                && estado.ValueKind == JsonValueKind.String && estado.GetString() == "FINALIZADA"
                && HttpContext.Bearer() is { } bearer)
                await sync.RefrescarCacheAsync(bearer);
        }
        return new ContentResult { StatusCode = r.StatusCode, Content = r.Body, ContentType = "application/json" };
    }

    [HttpPost("{id:int}/bomberos")]
    public async Task<IActionResult> AsignarBombero(int id, [FromBody] JsonElement asignacion)
    {
        if (!TryGetId(asignacion, "bomberoId", out var bomberoId))
            return BadRequest(Respuestas.Error("El bombero es requerido."));

        var r = await operaciones.EjecutarOEncolarAsync(HttpContext, TiposOperacion.AsignarBombero, HttpMethod.Post,
            $"api/v1/emergencias/{id}/bomberos", asignacion.GetRawText(), (TiposRecurso.Bombero, bomberoId));
        if (r.AplicadaEnCore) await sync.RefrescarRecursoAsync(TiposRecurso.Bombero, bomberoId, HttpContext.Bearer());   // mantiene la copia al día
        return r.Respuesta;
    }

    [HttpPost("{id:int}/vehiculos")]
    public async Task<IActionResult> AsignarVehiculo(int id, [FromBody] JsonElement asignacion)
    {
        if (!TryGetId(asignacion, "vehiculoId", out var vehiculoId))
            return BadRequest(Respuestas.Error("El vehículo es requerido."));

        var r = await operaciones.EjecutarOEncolarAsync(HttpContext, TiposOperacion.AsignarVehiculo, HttpMethod.Post,
            $"api/v1/emergencias/{id}/vehiculos", asignacion.GetRawText(), (TiposRecurso.Vehiculo, vehiculoId));
        if (r.AplicadaEnCore) await sync.RefrescarRecursoAsync(TiposRecurso.Vehiculo, vehiculoId, HttpContext.Bearer());
        return r.Respuesta;
    }

    /// <summary>Finaliza o cancela la asignación de un bombero.</summary>
    [HttpPatch("{id:int}/bomberos/{bomberoId:int}/{accion:regex(^(finalizar|cancelar)$)}")]
    public async Task<IActionResult> CerrarBombero(int id, int bomberoId, string accion)
    {
        var r = await operaciones.EjecutarOEncolarAsync(HttpContext, TiposOperacion.FinalizarAsignacion, HttpMethod.Patch,
            $"api/v1/emergencias/{id}/bomberos/{bomberoId}/{accion}", null);
        if (r.AplicadaEnCore) await sync.RefrescarRecursoAsync(TiposRecurso.Bombero, bomberoId, HttpContext.Bearer());
        return r.Respuesta;
    }

    /// <summary>Finaliza o cancela la asignación de un vehículo.</summary>
    [HttpPatch("{id:int}/vehiculos/{vehiculoId:int}/{accion:regex(^(finalizar|cancelar)$)}")]
    public async Task<IActionResult> CerrarVehiculo(int id, int vehiculoId, string accion)
    {
        var r = await operaciones.EjecutarOEncolarAsync(HttpContext, TiposOperacion.FinalizarAsignacion, HttpMethod.Patch,
            $"api/v1/emergencias/{id}/vehiculos/{vehiculoId}/{accion}", null);
        if (r.AplicadaEnCore) await sync.RefrescarRecursoAsync(TiposRecurso.Vehiculo, vehiculoId, HttpContext.Bearer());
        return r.Respuesta;
    }

    // El sistema publica un servicio y también consume uno externo: el aviso de cambio de estado.
    private async Task AvisarAl911Async(string? emergenciaJson)
    {
        if (string.IsNullOrWhiteSpace(emergenciaJson)) return;
        using var doc = JsonDocument.Parse(emergenciaJson);
        var e = doc.RootElement;
        if (!e.TryGetProperty("origen", out var origen) || origen.GetString() != "CENTRAL_911") return;
        if (!e.TryGetProperty("referenciaExterna", out var referencia) || referencia.GetString() is not { Length: > 0 } valor) return;

        await cliente911.NotificarEstadoAsync(valor, e.GetProperty("estado").GetString() ?? "");
    }

    internal static string NuevaReferencia() => $"PROV-{Guid.NewGuid():N}"[..17].ToUpperInvariant();

    /// <summary>Valor de una propiedad de texto; un número u otro tipo se devuelve tal cual para que la validación lo rechace.</summary>
    private static string? Texto(JsonElement body, string campo)
    {
        if (body.ValueKind != JsonValueKind.Object) return null;
        var propiedad = body.EnumerateObject()
            .FirstOrDefault(p => string.Equals(p.Name, campo, StringComparison.OrdinalIgnoreCase));
        return propiedad.Value.ValueKind switch
        {
            JsonValueKind.String => propiedad.Value.GetString(),
            JsonValueKind.Null or JsonValueKind.Undefined => null,
            _ => propiedad.Value.GetRawText()
        };
    }

    private static bool TryGetId(JsonElement body, string campo, out int id)
    {
        id = 0;
        return body.ValueKind == JsonValueKind.Object
               && body.TryGetProperty(campo, out var v)
               && v.ValueKind == JsonValueKind.Number
               && v.TryGetInt32(out id) && id > 0;
    }
}
