using Bomberos.Core.Application.Common;
using Bomberos.Core.Application.DTOs.Usuarios;
using Bomberos.Core.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bomberos.WebApi.Controllers;

[ApiController]
[Route("api/v1/usuarios")]
[Authorize(Roles = Roles.Administrador)]
public class UsuariosController(IUsuarioService usuarios) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<UsuarioDto>>> GetAll() => Ok(await usuarios.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<ActionResult<UsuarioDto>> GetById(string id) => Ok(await usuarios.GetByIdAsync(id));

    [HttpPost]
    public async Task<ActionResult<UsuarioDto>> Crear(CrearUsuarioDto dto)
    {
        var creado = await usuarios.CrearAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = creado.Id }, creado);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<UsuarioDto>> Editar(string id, EditarUsuarioDto dto)
        => Ok(await usuarios.EditarAsync(id, dto));

    [HttpPatch("{id}/estado")]
    public async Task<ActionResult<UsuarioDto>> CambiarEstado(string id, CambiarEstadoUsuarioDto dto)
        => Ok(await usuarios.CambiarEstadoAsync(id, dto.Estado));

    [HttpPatch("{id}/rol")]
    public async Task<ActionResult<UsuarioDto>> CambiarRol(string id, CambiarRolDto dto)
        => Ok(await usuarios.CambiarRolAsync(id, dto.Rol));

    [HttpPatch("{id}/password")]
    public async Task<IActionResult> RestablecerPassword(string id, RestablecerPasswordDto dto)
    {
        await usuarios.RestablecerPasswordAsync(id, dto.NuevaPassword);
        return NoContent();
    }
}

[ApiController]
[Route("api/v1/roles")]
[Authorize(Roles = Roles.Administrador)]
public class RolesController(IRolService roles) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<RolDto>>> GetAll() => Ok(await roles.GetAllAsync());

    [HttpPost]
    public async Task<ActionResult<RolDto>> Crear(GuardarRolDto dto)
        => StatusCode(StatusCodes.Status201Created, await roles.CrearAsync(dto));

    [HttpPut("{id}")]
    public async Task<ActionResult<RolDto>> Editar(string id, GuardarRolDto dto) => Ok(await roles.EditarAsync(id, dto));

    [HttpDelete("{id}")]
    public async Task<IActionResult> Eliminar(string id)
    {
        await roles.EliminarAsync(id);
        return NoContent();
    }
}
