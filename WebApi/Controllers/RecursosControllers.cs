using Bomberos.Core.Application.Common;
using Bomberos.Core.Application.DTOs.Recursos;
using Bomberos.Core.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bomberos.WebApi.Controllers;

// Lectura: cualquier usuario autenticado. Escritura: ADMINISTRADOR.

[ApiController]
[Route("api/v1/estaciones")]
[Authorize]
public class EstacionesController(IEstacionService estaciones) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<EstacionDto>>> GetAll() => Ok(await estaciones.GetAllAsync());

    [HttpGet("{id:int}")]
    public async Task<ActionResult<EstacionDto>> GetById(int id) => Ok(await estaciones.GetByIdAsync(id));

    [HttpPost]
    [Authorize(Roles = Roles.Administrador)]
    public async Task<ActionResult<EstacionDto>> Crear(GuardarEstacionDto dto)
    {
        var creada = await estaciones.CrearAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = creada.Id }, creada);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.Administrador)]
    public async Task<ActionResult<EstacionDto>> Editar(int id, GuardarEstacionDto dto)
        => Ok(await estaciones.EditarAsync(id, dto));

    [HttpPatch("{id:int}/estado")]
    [Authorize(Roles = Roles.Administrador)]
    public async Task<ActionResult<EstacionDto>> CambiarEstado(int id, CambiarEstadoEstacionDto dto)
        => Ok(await estaciones.CambiarEstadoAsync(id, dto.Estado!.Value));
}

[ApiController]
[Route("api/v1/bomberos")]
[Authorize]
public class BomberosController(IBomberoService bomberos) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<BomberoDto>>> GetAll() => Ok(await bomberos.GetAllAsync());

    /// <summary>Bomberos ACTIVOS sin asignación activa.</summary>
    [HttpGet("disponibles")]
    public async Task<ActionResult<List<BomberoDto>>> GetDisponibles() => Ok(await bomberos.GetDisponiblesAsync());

    [HttpGet("{id:int}")]
    public async Task<ActionResult<BomberoDto>> GetById(int id) => Ok(await bomberos.GetByIdAsync(id));

    [HttpPost]
    [Authorize(Roles = Roles.Administrador)]
    public async Task<ActionResult<BomberoDto>> Crear(GuardarBomberoDto dto)
    {
        var creado = await bomberos.CrearAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = creado.Id }, creado);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.Administrador)]
    public async Task<ActionResult<BomberoDto>> Editar(int id, GuardarBomberoDto dto)
        => Ok(await bomberos.EditarAsync(id, dto));

    [HttpPatch("{id:int}/estado")]
    [Authorize(Roles = Roles.Administrador)]
    public async Task<ActionResult<BomberoDto>> CambiarEstado(int id, CambiarEstadoBomberoDto dto)
        => Ok(await bomberos.CambiarEstadoAsync(id, dto.Estado!.Value));
}

[ApiController]
[Route("api/v1/vehiculos")]
[Authorize]
public class VehiculosController(IVehiculoService vehiculos) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<VehiculoDto>>> GetAll() => Ok(await vehiculos.GetAllAsync());

    /// <summary>Vehículos en estado DISPONIBLE.</summary>
    [HttpGet("disponibles")]
    public async Task<ActionResult<List<VehiculoDto>>> GetDisponibles() => Ok(await vehiculos.GetDisponiblesAsync());

    [HttpGet("{id:int}")]
    public async Task<ActionResult<VehiculoDto>> GetById(int id) => Ok(await vehiculos.GetByIdAsync(id));

    [HttpPost]
    [Authorize(Roles = Roles.Administrador)]
    public async Task<ActionResult<VehiculoDto>> Crear(GuardarVehiculoDto dto)
    {
        var creado = await vehiculos.CrearAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = creado.Id }, creado);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.Administrador)]
    public async Task<ActionResult<VehiculoDto>> Editar(int id, GuardarVehiculoDto dto)
        => Ok(await vehiculos.EditarAsync(id, dto));

    // El operador también puede actualizar el estado de un vehículo.
    [HttpPatch("{id:int}/estado")]
    [Authorize(Roles = Roles.AdministradorOperador)]
    public async Task<ActionResult<VehiculoDto>> CambiarEstado(int id, CambiarEstadoVehiculoDto dto)
        => Ok(await vehiculos.CambiarEstadoAsync(id, dto.Estado!.Value));
}
