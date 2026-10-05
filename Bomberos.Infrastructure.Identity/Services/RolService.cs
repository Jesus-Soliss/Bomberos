using Bomberos.Core.Application.Common;
using Bomberos.Core.Application.DTOs.Usuarios;
using Bomberos.Core.Application.Exceptions;
using Bomberos.Core.Application.Interfaces.Services;
using Bomberos.Core.Application.Services;
using Bomberos.Infrastructure.Identity.Contexts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Bomberos.Infrastructure.Identity.Services;

public class RolService(RoleManager<IdentityRole> roleManager, IdentityContext context, IAuditService audit) : IRolService
{
    private const string Entidad = "Rol";

    public async Task<List<RolDto>> GetAllAsync()
        => await context.Roles.AsNoTracking()
            .OrderBy(r => r.Name)
            .Select(r => new RolDto
            {
                Id = r.Id,
                Nombre = r.Name!,
                Usuarios = context.UserRoles.Count(ur => ur.RoleId == r.Id)
            })
            .ToListAsync();

    public Task<RolDto> CrearAsync(GuardarRolDto dto) => audit.AuditarAsync($"CREAR_ROL:{Normalizar(dto.Nombre)}", Entidad, null, async () =>
    {
        var rol = new IdentityRole(Normalizar(dto.Nombre));
        UsuarioService.Verificar(await roleManager.CreateAsync(rol));
        return new RolDto { Id = rol.Id, Nombre = rol.Name! };
    });

    public async Task<RolDto> EditarAsync(string id, GuardarRolDto dto)
    {
        var rol = await ObtenerAsync(id);
        return await audit.AuditarAsync($"EDITAR_ROL:{rol.Name}", Entidad, null, async () =>
        {
            ValidarNoEsBase(rol);
            rol.Name = Normalizar(dto.Nombre);
            UsuarioService.Verificar(await roleManager.UpdateAsync(rol));
            return new RolDto { Id = rol.Id, Nombre = rol.Name, Usuarios = await ContarUsuariosAsync(rol.Id) };
        });
    }

    public async Task EliminarAsync(string id)
    {
        var rol = await ObtenerAsync(id);
        await audit.AuditarAsync($"ELIMINAR_ROL:{rol.Name}", Entidad, null, async () =>
        {
            ValidarNoEsBase(rol);
            if (await ContarUsuariosAsync(rol.Id) > 0)
                throw new BusinessException("No se puede eliminar un perfil que tiene usuarios asignados.");

            UsuarioService.Verificar(await roleManager.DeleteAsync(rol));
            return true;
        });
    }

    private async Task<IdentityRole> ObtenerAsync(string id)
        => await roleManager.FindByIdAsync(id) ?? throw new NotFoundException("Perfil no encontrado.");

    private Task<int> ContarUsuariosAsync(string rolId) => context.UserRoles.CountAsync(ur => ur.RoleId == rolId);

    // Los permisos del sistema dependen de los tres perfiles base.
    private static void ValidarNoEsBase(IdentityRole rol)
    {
        if (Roles.Base.Contains(rol.Name))
            throw new BusinessException("Los perfiles base del sistema no se pueden modificar ni eliminar.");
    }

    private static string Normalizar(string nombre) => nombre.Trim().ToUpperInvariant();
}
