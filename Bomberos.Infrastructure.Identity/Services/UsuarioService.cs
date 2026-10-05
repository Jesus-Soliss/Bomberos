using Bomberos.Core.Application.Common;
using Bomberos.Core.Application.DTOs.Usuarios;
using Bomberos.Core.Application.Exceptions;
using Bomberos.Core.Application.Interfaces.Services;
using Bomberos.Core.Application.Services;
using Bomberos.Infrastructure.Identity.Contexts;
using Bomberos.Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Bomberos.Infrastructure.Identity.Services;

public class UsuarioService(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager,
    IdentityContext context,
    ICurrentUserService currentUser,
    IAuditService audit) : IUsuarioService
{
    private const string Entidad = "Usuario";

    public async Task<List<UsuarioDto>> GetAllAsync()
    {
        var roles = await (from ur in context.UserRoles
                           join r in context.Roles on ur.RoleId equals r.Id
                           select new { ur.UserId, r.Name }).ToListAsync();
        var rolPorUsuario = roles.GroupBy(x => x.UserId).ToDictionary(g => g.Key, g => g.First().Name);

        var usuarios = await context.Users.AsNoTracking().OrderBy(u => u.UserName).ToListAsync();
        return usuarios.Select(u => ADto(u, rolPorUsuario.GetValueOrDefault(u.Id))).ToList();
    }

    public async Task<UsuarioDto> GetByIdAsync(string id) => await ADtoAsync(await ObtenerAsync(id));

    public Task<UsuarioDto> CrearAsync(CrearUsuarioDto dto) => audit.AuditarAsync($"CREAR_USUARIO:{dto.Username.Trim()}", Entidad, null, async () =>
    {
        var rol = await ObtenerRolAsync(dto.Rol);

        var user = new ApplicationUser
        {
            UserName = dto.Username.Trim(),
            Email = dto.Email.Trim(),
            Nombre = dto.Nombre.Trim(),
            Apellido = dto.Apellido.Trim(),
            Estado = EstadosUsuario.Activo,
            EmailConfirmed = true
        };

        // RN-13 y RN-22 las valida Identity (usuario único y política de contraseñas).
        Verificar(await userManager.CreateAsync(user, dto.Password));
        Verificar(await userManager.AddToRoleAsync(user, rol));
        return ADto(user, rol);
    });

    public async Task<UsuarioDto> EditarAsync(string id, EditarUsuarioDto dto)
    {
        var user = await ObtenerAsync(id);
        return await audit.AuditarAsync($"EDITAR_USUARIO:{user.UserName}", Entidad, null, async () =>
        {
            user.Email = dto.Email.Trim();
            user.Nombre = dto.Nombre.Trim();
            user.Apellido = dto.Apellido.Trim();
            Verificar(await userManager.UpdateAsync(user));
            return await ADtoAsync(user);
        });
    }

    // RN-14: el usuario no se elimina, se inactiva.
    public async Task<UsuarioDto> CambiarEstadoAsync(string id, string estado)
    {
        estado = estado.Trim().ToUpperInvariant();
        if (estado is not (EstadosUsuario.Activo or EstadosUsuario.Inactivo))
            throw new BusinessException("El estado debe ser ACTIVO o INACTIVO.");

        var user = await ObtenerAsync(id);
        var accion = estado == EstadosUsuario.Activo ? "ACTIVAR_USUARIO" : "INACTIVAR_USUARIO";
        return await audit.AuditarAsync($"{accion}:{user.UserName}", Entidad, null, async () =>
        {
            // RN-16
            if (estado == EstadosUsuario.Inactivo && user.Id == currentUser.UserId)
                throw new BusinessException("No puede inactivar su propio usuario.");

            user.Estado = estado;
            Verificar(await userManager.UpdateAsync(user));
            return await ADtoAsync(user);
        });
    }

    public async Task<UsuarioDto> CambiarRolAsync(string id, string rol)
    {
        var user = await ObtenerAsync(id);
        return await audit.AuditarAsync($"CAMBIAR_ROL_USUARIO:{user.UserName}", Entidad, null, async () =>
        {
            // RN-16
            if (user.Id == currentUser.UserId)
                throw new BusinessException("No puede cambiar su propio rol.");

            var nuevoRol = await ObtenerRolAsync(rol);
            var actuales = await userManager.GetRolesAsync(user);
            if (actuales.Count > 0) Verificar(await userManager.RemoveFromRolesAsync(user, actuales));
            Verificar(await userManager.AddToRoleAsync(user, nuevoRol));
            return ADto(user, nuevoRol);
        });
    }

    public async Task RestablecerPasswordAsync(string id, string nuevaPassword)
    {
        var user = await ObtenerAsync(id);
        await audit.AuditarAsync($"RESTABLECER_PASSWORD:{user.UserName}", Entidad, null, async () =>
        {
            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            Verificar(await userManager.ResetPasswordAsync(user, token, nuevaPassword));
            // Quita un posible bloqueo por intentos fallidos.
            await userManager.SetLockoutEndDateAsync(user, null);
            await userManager.ResetAccessFailedCountAsync(user);
            return true;
        });
    }

    private async Task<ApplicationUser> ObtenerAsync(string id)
        => await userManager.FindByIdAsync(id) ?? throw new NotFoundException("Usuario no encontrado.");

    private async Task<string> ObtenerRolAsync(string rol)
    {
        var encontrado = await roleManager.FindByNameAsync(rol.Trim());
        return encontrado?.Name ?? throw new BusinessException($"El perfil '{rol}' no existe.");
    }

    private async Task<UsuarioDto> ADtoAsync(ApplicationUser user)
        => ADto(user, (await userManager.GetRolesAsync(user)).FirstOrDefault());

    private static UsuarioDto ADto(ApplicationUser user, string? rol) => new()
    {
        Id = user.Id,
        Username = user.UserName ?? "",
        Email = user.Email ?? "",
        Nombre = user.Nombre,
        Apellido = user.Apellido,
        Estado = user.Estado,
        Rol = rol
    };

    internal static void Verificar(IdentityResult result)
    {
        if (result.Succeeded) return;
        var mensaje = string.Join(" ", result.Errors.Select(e => e.Description));
        if (result.Errors.Any(e => e.Code.StartsWith("Duplicate", StringComparison.Ordinal)))
            throw new ConflictException(mensaje);
        throw new BusinessException(mensaje);
    }
}
