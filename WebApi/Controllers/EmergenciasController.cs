using Bomberos.Core.Application.Common;
using Bomberos.Core.Application.DTOs.Emergencias;
using Bomberos.Core.Application.Interfaces.Services;
using Bomberos.Core.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bomberos.WebApi.Controllers;

// Lectura: personal del sistema (cualquier perfil menos CLIENTE). Escritura y asignaciones: ADMINISTRADOR y OPERADOR.
// Registrar una emergencia: también CLIENTE, que no tiene acceso a nada más.
[ApiController]
[Route("api/v1/emergencias")]
[Authorize]
public class EmergenciasController(IEmergenciaService emergencias, IAsignacionService asignaciones) : ControllerBase
{
    /// <summary>Filtros opcionales: estado, tipo, prioridad, origen, desde, hasta.</summary>
    [HttpGet]
    [Authorize(Policy = Politicas.Personal)]
    public async Task<ActionResult<List<EmergenciaDto>>> Buscar([FromQuery] EmergenciaFiltroDto filtro)
        => Ok(await emergencias.BuscarAsync(filtro));

    /// <summary>Detalle con el historial de asignaciones (activas y cerradas).</summary>
    [HttpGet("{id:int}")]
    [Authorize(Policy = Politicas.Personal)]
    public async Task<ActionResult<EmergenciaDetalleDto>> GetById(int id) => Ok(await emergencias.GetDetalleAsync(id));

    /// <summary>Consulta por la referencia del Centro 911.</summary>
    [HttpGet("externa/{referencia}")]
    [Authorize(Policy = Politicas.Personal)]
    public async Task<ActionResult<EmergenciaDto>> GetPorReferencia(string referencia)
        => Ok(await emergencias.GetPorReferenciaAsync(referencia));

    [HttpPost]
    [Authorize(Roles = Roles.RegistranEmergencias)]
    public async Task<ActionResult<EmergenciaDto>> Crear(GuardarEmergenciaDto dto)
    {
        var creada = await emergencias.CrearAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = creada.Id }, creada);
    }

    /// <summary>Reporte del Centro 911. Lo invoca Integración con su cuenta de servicio. 409 si la referencia ya existe.</summary>
    [HttpPost("externa")]
    [Authorize(Roles = Roles.AdministradorOperador)]
    public async Task<ActionResult<EmergenciaDto>> CrearExterna(ReporteExternoDto dto)
    {
        var creada = await emergencias.CrearExternaAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = creada.Id }, creada);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.AdministradorOperador)]
    public async Task<ActionResult<EmergenciaDto>> Editar(int id, GuardarEmergenciaDto dto)
        => Ok(await emergencias.EditarAsync(id, dto));

    /// <summary>Cambia el estado (EN_ATENCION, CONTROLADA, FINALIZADA) o cancela (CANCELADA).</summary>
    [HttpPatch("{id:int}/estado")]
    [Authorize(Roles = Roles.AdministradorOperador)]
    public async Task<ActionResult<EmergenciaDto>> CambiarEstado(int id, CambiarEstadoEmergenciaDto dto)
        => Ok(await emergencias.CambiarEstadoAsync(id, dto.Estado!.Value));

    // ---------- Asignaciones ----------

    [HttpPost("{id:int}/bomberos")]
    [Authorize(Roles = Roles.AdministradorOperador)]
    public async Task<ActionResult<AsignacionBomberoDto>> AsignarBombero(int id, AsignarBomberoDto dto)
        => StatusCode(StatusCodes.Status201Created, await asignaciones.AsignarBomberoAsync(id, dto.BomberoId));

    [HttpPost("{id:int}/vehiculos")]
    [Authorize(Roles = Roles.AdministradorOperador)]
    public async Task<ActionResult<AsignacionVehiculoDto>> AsignarVehiculo(int id, AsignarVehiculoDto dto)
        => StatusCode(StatusCodes.Status201Created, await asignaciones.AsignarVehiculoAsync(id, dto.VehiculoId));

    [HttpPatch("{id:int}/bomberos/{bomberoId:int}/finalizar")]
    [Authorize(Roles = Roles.AdministradorOperador)]
    public async Task<ActionResult<AsignacionBomberoDto>> FinalizarBombero(int id, int bomberoId)
        => Ok(await asignaciones.CerrarBomberoAsync(id, bomberoId, EstadoAsignacion.Finalizada));

    [HttpPatch("{id:int}/bomberos/{bomberoId:int}/cancelar")]
    [Authorize(Roles = Roles.AdministradorOperador)]
    public async Task<ActionResult<AsignacionBomberoDto>> CancelarBombero(int id, int bomberoId)
        => Ok(await asignaciones.CerrarBomberoAsync(id, bomberoId, EstadoAsignacion.Cancelada));

    [HttpPatch("{id:int}/vehiculos/{vehiculoId:int}/finalizar")]
    [Authorize(Roles = Roles.AdministradorOperador)]
    public async Task<ActionResult<AsignacionVehiculoDto>> FinalizarVehiculo(int id, int vehiculoId)
        => Ok(await asignaciones.CerrarVehiculoAsync(id, vehiculoId, EstadoAsignacion.Finalizada));

    [HttpPatch("{id:int}/vehiculos/{vehiculoId:int}/cancelar")]
    [Authorize(Roles = Roles.AdministradorOperador)]
    public async Task<ActionResult<AsignacionVehiculoDto>> CancelarVehiculo(int id, int vehiculoId)
        => Ok(await asignaciones.CerrarVehiculoAsync(id, vehiculoId, EstadoAsignacion.Cancelada));
}
