using AutoMapper;
using Bomberos.Core.Application.DTOs.Recursos;
using Bomberos.Core.Application.Exceptions;
using Bomberos.Core.Application.Interfaces.Repositories;
using Bomberos.Core.Application.Interfaces.Services;
using Bomberos.Core.Domain.Entities;
using Bomberos.Core.Domain.Enums;

namespace Bomberos.Core.Application.Services;

public class EstacionService(IEstacionRepository estaciones, IAuditService audit, IMapper mapper) : IEstacionService
{
    private const string Entidad = nameof(Estacion);

    public async Task<List<EstacionDto>> GetAllAsync()
        => mapper.Map<List<EstacionDto>>((await estaciones.GetAllAsync()).OrderBy(e => e.Nombre));

    public async Task<EstacionDto> GetByIdAsync(int id) => mapper.Map<EstacionDto>(await ObtenerAsync(id));

    public Task<EstacionDto> CrearAsync(GuardarEstacionDto dto) => audit.AuditarAsync("CREAR_ESTACION", Entidad, null, async () =>
    {
        Normalizar(dto);
        if (await estaciones.ExisteNombreAsync(dto.Nombre))
            throw new ConflictException("Ya existe una estación con ese nombre.");

        var estacion = mapper.Map<Estacion>(dto);
        estacion.Estado = EstadoEstacion.Activa;
        await estaciones.AddAsync(estacion);
        return mapper.Map<EstacionDto>(estacion);
    }, r => r.Id);

    public Task<EstacionDto> EditarAsync(int id, GuardarEstacionDto dto) => audit.AuditarAsync("EDITAR_ESTACION", Entidad, id, async () =>
    {
        Normalizar(dto);
        var estacion = await ObtenerAsync(id);
        if (await estaciones.ExisteNombreAsync(dto.Nombre, id))
            throw new ConflictException("Ya existe una estación con ese nombre.");

        mapper.Map(dto, estacion);
        await estaciones.UpdateAsync(estacion);
        return mapper.Map<EstacionDto>(estacion);
    });

    // RN-14: la estación no se elimina, se inactiva.
    public Task<EstacionDto> CambiarEstadoAsync(int id, EstadoEstacion estado)
        => audit.AuditarAsync(estado == EstadoEstacion.Activa ? "ACTIVAR_ESTACION" : "INACTIVAR_ESTACION", Entidad, id, async () =>
        {
            var estacion = await ObtenerAsync(id);
            estacion.Estado = estado;
            await estaciones.UpdateAsync(estacion);
            return mapper.Map<EstacionDto>(estacion);
        });

    private async Task<Estacion> ObtenerAsync(int id)
        => await estaciones.GetByIdAsync(id) ?? throw new NotFoundException("Estación no encontrada.");

    private static void Normalizar(GuardarEstacionDto dto)
    {
        dto.Nombre = dto.Nombre.Trim();
        dto.Direccion = dto.Direccion.Trim();
        dto.Telefono = string.IsNullOrWhiteSpace(dto.Telefono) ? null : dto.Telefono.Trim();
    }
}
