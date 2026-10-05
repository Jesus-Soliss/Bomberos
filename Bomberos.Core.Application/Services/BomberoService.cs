using AutoMapper;
using Bomberos.Core.Application.DTOs.Recursos;
using Bomberos.Core.Application.Exceptions;
using Bomberos.Core.Application.Interfaces.Repositories;
using Bomberos.Core.Application.Interfaces.Services;
using Bomberos.Core.Domain.Entities;
using Bomberos.Core.Domain.Enums;

namespace Bomberos.Core.Application.Services;

public class BomberoService(
    IBomberoRepository bomberos,
    IEstacionRepository estaciones,
    IAsignacionRepository asignaciones,
    IAuditService audit,
    IMapper mapper) : IBomberoService
{
    private const string Entidad = nameof(Bombero);

    public async Task<List<BomberoDto>> GetAllAsync()
    {
        var dtos = mapper.Map<List<BomberoDto>>(await bomberos.GetAllConEstacionAsync());
        var enServicio = await asignaciones.GetBomberosEnServicioAsync();
        foreach (var dto in dtos) dto.EnServicio = enServicio.Contains(dto.Id);
        return dtos;
    }

    public async Task<List<BomberoDto>> GetDisponiblesAsync()
        => mapper.Map<List<BomberoDto>>(await bomberos.GetDisponiblesAsync());

    public async Task<BomberoDto> GetByIdAsync(int id) => await ADtoAsync(await ObtenerAsync(id));

    public Task<BomberoDto> CrearAsync(GuardarBomberoDto dto) => audit.AuditarAsync("CREAR_BOMBERO", Entidad, null, async () =>
    {
        Normalizar(dto);
        await ValidarAsync(dto, null);

        var bombero = mapper.Map<Bombero>(dto);
        bombero.Estado = EstadoBombero.Activo;
        await bomberos.AddAsync(bombero);
        return await ADtoAsync(await ObtenerAsync(bombero.Id));
    }, r => r.Id);

    public Task<BomberoDto> EditarAsync(int id, GuardarBomberoDto dto) => audit.AuditarAsync("EDITAR_BOMBERO", Entidad, id, async () =>
    {
        Normalizar(dto);
        var bombero = await ObtenerAsync(id);
        await ValidarAsync(dto, bombero);

        mapper.Map(dto, bombero);
        await bomberos.UpdateAsync(bombero);
        return await ADtoAsync(await ObtenerAsync(id));
    });

    // RN-14: el bombero no se elimina, se inactiva.
    public Task<BomberoDto> CambiarEstadoAsync(int id, EstadoBombero estado)
        => audit.AuditarAsync(estado == EstadoBombero.Activo ? "ACTIVAR_BOMBERO" : "INACTIVAR_BOMBERO", Entidad, id, async () =>
        {
            var bombero = await ObtenerAsync(id);
            // RN-21
            if (estado == EstadoBombero.Inactivo && await asignaciones.BomberoTieneAsignacionActivaAsync(id))
                throw new BusinessException("No se puede inactivar un bombero con una asignación activa.");

            bombero.Estado = estado;
            await bomberos.UpdateAsync(bombero);
            return await ADtoAsync(bombero);
        });

    private async Task<Bombero> ObtenerAsync(int id)
        => await bomberos.GetConEstacionAsync(id) ?? throw new NotFoundException("Bombero no encontrado.");

    private async Task<BomberoDto> ADtoAsync(Bombero bombero)
    {
        var dto = mapper.Map<BomberoDto>(bombero);
        dto.EnServicio = await asignaciones.BomberoTieneAsignacionActivaAsync(bombero.Id);
        return dto;
    }

    private async Task ValidarAsync(GuardarBomberoDto dto, Bombero? actual)
    {
        // RN-13
        if (await bomberos.ExisteCedulaAsync(dto.Cedula, actual?.Id))
            throw new ConflictException("Ya existe un bombero con esa cédula.");

        if (actual?.EstacionId == dto.EstacionId) return;
        var estacion = await estaciones.GetByIdAsync(dto.EstacionId)
            ?? throw new BusinessException("La estación indicada no existe.");
        if (estacion.Estado != EstadoEstacion.Activa)
            throw new BusinessException("La estación indicada está inactiva.");
    }

    private static void Normalizar(GuardarBomberoDto dto)
    {
        dto.Nombre = dto.Nombre.Trim();
        dto.Apellido = dto.Apellido.Trim();
        dto.Cedula = dto.Cedula.Trim();
    }
}
