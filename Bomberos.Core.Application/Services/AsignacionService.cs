using AutoMapper;
using Bomberos.Core.Application.DTOs.Emergencias;
using Bomberos.Core.Application.Exceptions;
using Bomberos.Core.Application.Interfaces.Repositories;
using Bomberos.Core.Application.Interfaces.Services;
using Bomberos.Core.Domain.Common;
using Bomberos.Core.Domain.Entities;
using Bomberos.Core.Domain.Enums;

namespace Bomberos.Core.Application.Services;

public class AsignacionService(
    IEmergenciaRepository emergencias,
    IBomberoRepository bomberos,
    IVehiculoRepository vehiculos,
    IAsignacionRepository asignaciones,
    IUnitOfWork unitOfWork,
    IAuditService audit,
    IMapper mapper) : IAsignacionService
{
    public Task<AsignacionBomberoDto> AsignarBomberoAsync(int emergenciaId, int bomberoId)
        => audit.AuditarAsync("ASIGNAR_BOMBERO", nameof(Emergencia), emergenciaId, async () =>
        {
            var emergencia = await ObtenerEmergenciaAsignableAsync(emergenciaId);

            var bombero = await bomberos.GetByIdAsync(bomberoId) ?? throw new NotFoundException("Bombero no encontrado.");
            // RN-01
            if (bombero.Estado != EstadoBombero.Activo)
                throw new BusinessException("El bombero está inactivo.");
            // RN-10
            if (await asignaciones.GetBomberoAsync(emergenciaId, bomberoId) is not null)
                throw new ConflictException("El bombero ya tiene un registro de asignación en esta emergencia.");
            // RN-03
            if (await asignaciones.BomberoTieneAsignacionActivaAsync(bomberoId))
                throw new BusinessException("El bombero ya está asignado a otra emergencia activa.");

            var asignacion = new AsignacionBombero
            {
                EmergenciaId = emergenciaId,
                BomberoId = bomberoId,
                FechaAsignacion = DateTime.UtcNow,
                Estado = EstadoAsignacion.Activa
            };

            await unitOfWork.EjecutarEnTransaccionAsync(async () =>
            {
                await asignaciones.AddBomberoAsync(asignacion);
                await PasarAEnAtencionAsync(emergencia);
            });

            asignacion.Bombero = bombero;
            return mapper.Map<AsignacionBomberoDto>(asignacion);
        });

    public Task<AsignacionVehiculoDto> AsignarVehiculoAsync(int emergenciaId, int vehiculoId)
        => audit.AuditarAsync("ASIGNAR_VEHICULO", nameof(Emergencia), emergenciaId, async () =>
        {
            var emergencia = await ObtenerEmergenciaAsignableAsync(emergenciaId);

            var vehiculo = await vehiculos.GetByIdAsync(vehiculoId) ?? throw new NotFoundException("Vehículo no encontrado.");
            // RN-10
            if (await asignaciones.GetVehiculoAsync(emergenciaId, vehiculoId) is not null)
                throw new ConflictException("El vehículo ya tiene un registro de asignación en esta emergencia.");
            // RN-04
            if (await asignaciones.VehiculoTieneAsignacionActivaAsync(vehiculoId))
                throw new BusinessException("El vehículo ya está asignado a otra emergencia activa.");
            // RN-02 y RN-17
            if (vehiculo.Estado != EstadoVehiculo.Disponible)
                throw new BusinessException($"El vehículo no está disponible (estado {vehiculo.Estado.ToTexto()}).");

            var asignacion = new AsignacionVehiculo
            {
                EmergenciaId = emergenciaId,
                VehiculoId = vehiculoId,
                FechaAsignacion = DateTime.UtcNow,
                Estado = EstadoAsignacion.Activa
            };

            await unitOfWork.EjecutarEnTransaccionAsync(async () =>
            {
                await asignaciones.AddVehiculoAsync(asignacion);
                vehiculo.Estado = EstadoVehiculo.EnServicio;   // RN-07
                await vehiculos.UpdateAsync(vehiculo);
                await PasarAEnAtencionAsync(emergencia);
            });

            asignacion.Vehiculo = vehiculo;
            return mapper.Map<AsignacionVehiculoDto>(asignacion);
        });

    public Task<AsignacionBomberoDto> CerrarBomberoAsync(int emergenciaId, int bomberoId, EstadoAsignacion nuevoEstado)
        => audit.AuditarAsync(AccionDeCierre(nuevoEstado, "BOMBERO"), nameof(Emergencia), emergenciaId, async () =>
        {
            var asignacion = await asignaciones.GetBomberoAsync(emergenciaId, bomberoId)
                ?? throw new NotFoundException("El bombero no tiene una asignación en esta emergencia.");
            ValidarCierre(asignacion.Estado);

            asignacion.Estado = nuevoEstado;
            asignacion.FechaFinalizacion = DateTime.UtcNow;   // RN-09
            await asignaciones.UpdateBomberoAsync(asignacion);
            return mapper.Map<AsignacionBomberoDto>(asignacion);
        });

    public Task<AsignacionVehiculoDto> CerrarVehiculoAsync(int emergenciaId, int vehiculoId, EstadoAsignacion nuevoEstado)
        => audit.AuditarAsync(AccionDeCierre(nuevoEstado, "VEHICULO"), nameof(Emergencia), emergenciaId, async () =>
        {
            var asignacion = await asignaciones.GetVehiculoAsync(emergenciaId, vehiculoId)
                ?? throw new NotFoundException("El vehículo no tiene una asignación en esta emergencia.");
            ValidarCierre(asignacion.Estado);

            asignacion.Estado = nuevoEstado;
            asignacion.FechaFinalizacion = DateTime.UtcNow;   // RN-09

            await unitOfWork.EjecutarEnTransaccionAsync(async () =>
            {
                await asignaciones.UpdateVehiculoAsync(asignacion);
                asignacion.Vehiculo.Estado = EstadoVehiculo.Disponible;   // RN-07
                await vehiculos.UpdateAsync(asignacion.Vehiculo);
            });

            return mapper.Map<AsignacionVehiculoDto>(asignacion);
        });

    public async Task<AsignacionesDto> ListarAsync(string? estado)
    {
        EstadoAsignacion? filtro = null;
        if (!string.IsNullOrWhiteSpace(estado))
        {
            if (!EnumTexto.TryParse<EstadoAsignacion>(estado, out var valor))
                throw new BusinessException($"El valor '{estado}' no es válido para el filtro estado.");
            filtro = valor;
        }

        return new AsignacionesDto
        {
            Bomberos = mapper.Map<List<AsignacionBomberoDto>>(await asignaciones.ListarBomberosAsync(filtro)),
            Vehiculos = mapper.Map<List<AsignacionVehiculoDto>>(await asignaciones.ListarVehiculosAsync(filtro))
        };
    }

    // RN-05
    private async Task<Emergencia> ObtenerEmergenciaAsignableAsync(int emergenciaId)
    {
        var emergencia = await emergencias.GetByIdAsync(emergenciaId) ?? throw new NotFoundException("Emergencia no encontrada.");
        if (!emergencia.AdmiteAsignaciones)
            throw new BusinessException("Solo se pueden asignar recursos a emergencias pendientes o en atención.");
        return emergencia;
    }

    // RN-06
    private async Task PasarAEnAtencionAsync(Emergencia emergencia)
    {
        if (emergencia.Estado != EstadoEmergencia.Pendiente) return;
        emergencia.Estado = EstadoEmergencia.EnAtencion;
        await emergencias.UpdateAsync(emergencia);
    }

    private static void ValidarCierre(EstadoAsignacion actual)
    {
        if (actual != EstadoAsignacion.Activa)
            throw new BusinessException($"La asignación ya está {actual.ToTexto()}.");
    }

    private static string AccionDeCierre(EstadoAsignacion nuevoEstado, string recurso) => nuevoEstado switch
    {
        EstadoAsignacion.Finalizada => $"FINALIZAR_ASIGNACION_{recurso}",
        EstadoAsignacion.Cancelada => $"CANCELAR_ASIGNACION_{recurso}",
        _ => throw new BusinessException("Una asignación solo puede pasar a FINALIZADA o CANCELADA.")
    };
}
