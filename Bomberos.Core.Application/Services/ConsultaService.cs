using AutoMapper;
using Bomberos.Core.Application.DTOs.Consultas;
using Bomberos.Core.Application.DTOs.Emergencias;
using Bomberos.Core.Application.Interfaces.Repositories;
using Bomberos.Core.Application.Interfaces.Services;
using Bomberos.Core.Domain.Common;
using Bomberos.Core.Domain.Enums;

namespace Bomberos.Core.Application.Services;

public class ConsultaService(
    IEmergenciaRepository emergencias,
    IBomberoRepository bomberos,
    IVehiculoRepository vehiculos,
    IAsignacionRepository asignaciones,
    ILogRepository logs,
    IMapper mapper) : IConsultaService
{
    private const int LimiteMaximo = 1000;

    public async Task<DashboardDto> GetDashboardAsync()
    {
        var porEstado = await emergencias.ContarPorEstadoAsync();
        var porOrigen = await emergencias.ContarPorOrigenAsync();
        var vehiculosPorEstado = await vehiculos.ContarPorEstadoAsync();
        var bomberosActivos = await bomberos.ContarActivosAsync();
        var bomberosEnServicio = (await asignaciones.GetBomberosEnServicioAsync()).Count;

        return new DashboardDto
        {
            TotalEmergencias = porEstado.Values.Sum(),
            EmergenciasHoy = await emergencias.ContarAsync(desde: DateTime.UtcNow.Date),
            EmergenciasPorEstado = Enum.GetValues<EstadoEmergencia>().ToDictionary(e => e.ToTexto(), e => porEstado.GetValueOrDefault(e)),
            EmergenciasPorOrigen = Enum.GetValues<OrigenEmergencia>().ToDictionary(o => o.ToTexto(), o => porOrigen.GetValueOrDefault(o)),
            BomberosDisponibles = bomberosActivos - bomberosEnServicio,
            BomberosEnServicio = bomberosEnServicio,
            VehiculosDisponibles = vehiculosPorEstado.GetValueOrDefault(EstadoVehiculo.Disponible),
            VehiculosEnServicio = vehiculosPorEstado.GetValueOrDefault(EstadoVehiculo.EnServicio),
            VehiculosEnMantenimiento = vehiculosPorEstado.GetValueOrDefault(EstadoVehiculo.Mantenimiento),
            UltimasEmergencias = mapper.Map<List<EmergenciaDto>>(await emergencias.GetUltimasAsync(10))
        };
    }

    public async Task<List<AuditLogDto>> GetAuditoriaAsync(LogFiltroDto filtro)
        => mapper.Map<List<AuditLogDto>>(await logs.BuscarAuditoriaAsync(Acotar(filtro)));

    public async Task<List<RequestLogDto>> GetRequestLogsAsync(LogFiltroDto filtro)
        => mapper.Map<List<RequestLogDto>>(await logs.BuscarPeticionesAsync(Acotar(filtro)));

    private static LogFiltroDto Acotar(LogFiltroDto filtro)
    {
        filtro.Limite = Math.Clamp(filtro.Limite, 1, LimiteMaximo);
        return filtro;
    }
}
