using Bomberos.Core.Application.Common;
using Bomberos.Core.Application.DTOs.Consultas;
using Bomberos.Core.Application.DTOs.Emergencias;
using Bomberos.Core.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bomberos.WebApi.Controllers;

[ApiController]
[Route("api/v1")]
[Authorize]
public class ConsultasController(IConsultaService consultas, IAsignacionService asignaciones) : ControllerBase
{
    [HttpGet("dashboard")]
    public async Task<ActionResult<DashboardDto>> Dashboard() => Ok(await consultas.GetDashboardAsync());

    /// <summary>Asignaciones de todas las emergencias. Filtro opcional: estado (ACTIVA, FINALIZADA, CANCELADA).</summary>
    [HttpGet("asignaciones")]
    public async Task<ActionResult<AsignacionesDto>> Asignaciones([FromQuery] string? estado)
        => Ok(await asignaciones.ListarAsync(estado));

    /// <summary>Filtros opcionales: usuarioId, accion, desde, hasta, limite.</summary>
    [HttpGet("auditoria")]
    [Authorize(Roles = Roles.Administrador)]
    public async Task<ActionResult<List<AuditLogDto>>> Auditoria([FromQuery] LogFiltroDto filtro)
        => Ok(await consultas.GetAuditoriaAsync(filtro));

    /// <summary>Filtros opcionales: usuarioId, accion (parte del endpoint), desde, hasta, limite.</summary>
    [HttpGet("request-logs")]
    [Authorize(Roles = Roles.Administrador)]
    public async Task<ActionResult<List<RequestLogDto>>> RequestLogs([FromQuery] LogFiltroDto filtro)
        => Ok(await consultas.GetRequestLogsAsync(filtro));
}
