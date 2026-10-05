using AutoMapper;
using Bomberos.Core.Application.DTOs.Emergencias;
using Bomberos.Core.Application.Exceptions;
using Bomberos.Core.Application.Interfaces.Repositories;
using Bomberos.Core.Application.Interfaces.Services;
using Bomberos.Core.Domain.Common;
using Bomberos.Core.Domain.Entities;
using Bomberos.Core.Domain.Enums;

namespace Bomberos.Core.Application.Services;

public class EmergenciaService(
    IEmergenciaRepository emergencias,
    IAsignacionRepository asignaciones,
    IVehiculoRepository vehiculos,
    IUnitOfWork unitOfWork,
    IAuditService audit,
    IMapper mapper) : IEmergenciaService
{
    private const string Entidad = nameof(Emergencia);

    public async Task<List<EmergenciaDto>> BuscarAsync(EmergenciaFiltroDto filtro)
    {
        var criterio = new EmergenciaCriterio
        {
            Estado = Filtro<EstadoEmergencia>(filtro.Estado, "estado"),
            Tipo = Filtro<TipoEmergencia>(filtro.Tipo, "tipo"),
            Prioridad = Filtro<PrioridadEmergencia>(filtro.Prioridad, "prioridad"),
            Origen = Filtro<OrigenEmergencia>(filtro.Origen, "origen"),
            Desde = filtro.Desde,
            Hasta = filtro.Hasta
        };
        return mapper.Map<List<EmergenciaDto>>(await emergencias.BuscarAsync(criterio));
    }

    public async Task<EmergenciaDetalleDto> GetDetalleAsync(int id)
        => mapper.Map<EmergenciaDetalleDto>(
            await emergencias.GetDetalleAsync(id) ?? throw new NotFoundException("Emergencia no encontrada."));

    public async Task<EmergenciaDto> GetPorReferenciaAsync(string referenciaExterna)
        => mapper.Map<EmergenciaDto>(
            await emergencias.GetPorReferenciaAsync(referenciaExterna.Trim())
            ?? throw new NotFoundException("No existe una emergencia con esa referencia."));

    public Task<EmergenciaDto> CrearAsync(GuardarEmergenciaDto dto)
        => audit.AuditarAsync("CREAR_EMERGENCIA", Entidad, null, () => RegistrarAsync(dto, OrigenEmergencia.Operador, null), r => r.Id);

    // RN-19 y RN-20
    public Task<EmergenciaDto> CrearExternaAsync(ReporteExternoDto dto)
        => audit.AuditarAsync("REPORTE_911", Entidad, null, async () =>
        {
            var referencia = dto.Referencia.Trim();
            if (await emergencias.ExisteReferenciaAsync(referencia))
                throw new ConflictException("Ya existe una emergencia registrada con esa referencia externa.");

            return await RegistrarAsync(dto, OrigenEmergencia.Central911, referencia);
        }, r => r.Id);

    public Task<EmergenciaDto> EditarAsync(int id, GuardarEmergenciaDto dto) => audit.AuditarAsync("EDITAR_EMERGENCIA", Entidad, id, async () =>
    {
        var emergencia = await ObtenerAsync(id);
        if (emergencia.EstaCerrada)
            throw new BusinessException("Una emergencia finalizada o cancelada no se puede modificar.");

        mapper.Map(dto, emergencia);
        Normalizar(emergencia);
        await emergencias.UpdateAsync(emergencia);
        return mapper.Map<EmergenciaDto>(emergencia);
    });

    public Task<EmergenciaDto> CambiarEstadoAsync(int id, EstadoEmergencia estado)
        => audit.AuditarAsync($"CAMBIAR_ESTADO_EMERGENCIA_{estado.ToTexto()}", Entidad, id, async () =>
        {
            var emergencia = await ObtenerAsync(id);

            // RN-11
            if (emergencia.EstaCerrada)
                throw new BusinessException("Una emergencia finalizada o cancelada no puede cambiar de estado.");
            // RN-12
            if (estado == EstadoEmergencia.Cancelada && emergencia.Estado != EstadoEmergencia.Pendiente)
                throw new BusinessException("Solo se puede cancelar una emergencia en estado pendiente.");
            if (!emergencia.PuedeCambiarA(estado))
                throw new BusinessException(
                    $"No se puede pasar una emergencia de {emergencia.Estado.ToTexto()} a {estado.ToTexto()}.");

            emergencia.Estado = estado;
            if (estado == EstadoEmergencia.Finalizada)
                await FinalizarConAsignacionesAsync(emergencia);
            else
                await emergencias.UpdateAsync(emergencia);
            return mapper.Map<EmergenciaDto>(emergencia);
        });

    // RN-08: una emergencia finalizada no conserva asignaciones activas. En lugar de rechazar el cambio,
    // al finalizarla se finalizan también sus asignaciones activas (RN-09) y los vehículos se liberan (RN-07).
    private async Task FinalizarConAsignacionesAsync(Emergencia emergencia)
    {
        var bomberosActivos = await asignaciones.GetBomberosActivosAsync(emergencia.Id);
        var vehiculosActivos = await asignaciones.GetVehiculosActivosAsync(emergencia.Id);
        var ahora = DateTime.UtcNow;

        await unitOfWork.EjecutarEnTransaccionAsync(async () =>
        {
            foreach (var asignacion in bomberosActivos)
            {
                asignacion.Estado = EstadoAsignacion.Finalizada;
                asignacion.FechaFinalizacion = ahora;
                await asignaciones.UpdateBomberoAsync(asignacion);
            }

            foreach (var asignacion in vehiculosActivos)
            {
                asignacion.Estado = EstadoAsignacion.Finalizada;
                asignacion.FechaFinalizacion = ahora;
                await asignaciones.UpdateVehiculoAsync(asignacion);
                asignacion.Vehiculo.Estado = EstadoVehiculo.Disponible;
                await vehiculos.UpdateAsync(asignacion.Vehiculo);
            }

            await emergencias.UpdateAsync(emergencia);
        });
    }

    private async Task<EmergenciaDto> RegistrarAsync(GuardarEmergenciaDto dto, OrigenEmergencia origen, string? referencia)
    {
        var emergencia = mapper.Map<GuardarEmergenciaDto, Emergencia>(dto);
        Normalizar(emergencia);
        emergencia.FechaHora = DateTime.UtcNow;
        emergencia.Estado = EstadoEmergencia.Pendiente;
        emergencia.Origen = origen;
        emergencia.ReferenciaExterna = referencia;
        await emergencias.AddAsync(emergencia);
        return mapper.Map<EmergenciaDto>(emergencia);
    }

    private async Task<Emergencia> ObtenerAsync(int id)
        => await emergencias.GetByIdAsync(id) ?? throw new NotFoundException("Emergencia no encontrada.");

    private static void Normalizar(Emergencia emergencia)
    {
        emergencia.Descripcion = emergencia.Descripcion.Trim();
        emergencia.Direccion = emergencia.Direccion.Trim();
    }

    private static TEnum? Filtro<TEnum>(string? texto, string nombre) where TEnum : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(texto)) return null;
        return EnumTexto.TryParse<TEnum>(texto, out var valor)
            ? valor
            : throw new BusinessException($"El valor '{texto}' no es válido para el filtro {nombre}.");
    }
}
