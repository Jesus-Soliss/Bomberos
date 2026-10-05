using AutoMapper;
using Bomberos.Core.Application.DTOs.Recursos;
using Bomberos.Core.Application.Exceptions;
using Bomberos.Core.Application.Interfaces.Repositories;
using Bomberos.Core.Application.Interfaces.Services;
using Bomberos.Core.Domain.Entities;
using Bomberos.Core.Domain.Enums;

namespace Bomberos.Core.Application.Services;

public class VehiculoService(
    IVehiculoRepository vehiculos,
    IEstacionRepository estaciones,
    IAsignacionRepository asignaciones,
    IAuditService audit,
    IMapper mapper) : IVehiculoService
{
    private const string Entidad = nameof(Vehiculo);

    public async Task<List<VehiculoDto>> GetAllAsync()
        => mapper.Map<List<VehiculoDto>>(await vehiculos.GetAllConEstacionAsync());

    public async Task<List<VehiculoDto>> GetDisponiblesAsync()
        => mapper.Map<List<VehiculoDto>>(await vehiculos.GetDisponiblesAsync());

    public async Task<VehiculoDto> GetByIdAsync(int id) => mapper.Map<VehiculoDto>(await ObtenerAsync(id));

    public Task<VehiculoDto> CrearAsync(GuardarVehiculoDto dto) => audit.AuditarAsync("CREAR_VEHICULO", Entidad, null, async () =>
    {
        Normalizar(dto);
        await ValidarAsync(dto, null);

        var vehiculo = mapper.Map<Vehiculo>(dto);
        vehiculo.Estado = EstadoVehiculo.Disponible;
        await vehiculos.AddAsync(vehiculo);
        return mapper.Map<VehiculoDto>(await ObtenerAsync(vehiculo.Id));
    }, r => r.Id);

    public Task<VehiculoDto> EditarAsync(int id, GuardarVehiculoDto dto) => audit.AuditarAsync("EDITAR_VEHICULO", Entidad, id, async () =>
    {
        Normalizar(dto);
        var vehiculo = await ObtenerAsync(id);
        await ValidarAsync(dto, vehiculo);

        mapper.Map(dto, vehiculo);
        await vehiculos.UpdateAsync(vehiculo);
        return mapper.Map<VehiculoDto>(await ObtenerAsync(id));
    });

    public Task<VehiculoDto> CambiarEstadoAsync(int id, EstadoVehiculo estado)
        => audit.AuditarAsync("CAMBIAR_ESTADO_VEHICULO", Entidad, id, async () =>
        {
            var vehiculo = await ObtenerAsync(id);

            // EN_SERVICIO lo controlan las asignaciones (RN-07), no se cambia a mano.
            if (await asignaciones.VehiculoTieneAsignacionActivaAsync(id))
                throw new BusinessException("El vehículo tiene una asignación activa; finalícela antes de cambiar su estado.");
            if (estado == EstadoVehiculo.EnServicio)
                throw new BusinessException("El estado EN_SERVICIO se establece al asignar el vehículo a una emergencia.");

            vehiculo.Estado = estado;
            await vehiculos.UpdateAsync(vehiculo);
            return mapper.Map<VehiculoDto>(vehiculo);
        });

    private async Task<Vehiculo> ObtenerAsync(int id)
        => await vehiculos.GetConEstacionAsync(id) ?? throw new NotFoundException("Vehículo no encontrado.");

    private async Task ValidarAsync(GuardarVehiculoDto dto, Vehiculo? actual)
    {
        // RN-13
        if (await vehiculos.ExistePlacaAsync(dto.Placa, actual?.Id))
            throw new ConflictException("Ya existe un vehículo con esa placa.");

        if (actual?.EstacionId == dto.EstacionId) return;
        var estacion = await estaciones.GetByIdAsync(dto.EstacionId)
            ?? throw new BusinessException("La estación indicada no existe.");
        if (estacion.Estado != EstadoEstacion.Activa)
            throw new BusinessException("La estación indicada está inactiva.");
    }

    private static void Normalizar(GuardarVehiculoDto dto) => dto.Placa = dto.Placa.Trim().ToUpperInvariant();
}
