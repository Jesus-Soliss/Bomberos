using Bomberos.Core.Application.Common;
using Bomberos.Core.Application.DTOs.Auth;
using Bomberos.Core.Application.Interfaces.Services;
using Bomberos.Core.Domain.Enums;
using Bomberos.Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;

namespace Bomberos.Infrastructure.Identity.Services;

public class AuthService(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IJwtService jwtService,
    IAuditService audit) : IAuthService
{
    public async Task<AuthResponse> LoginAsync(AuthRequest req)
    {
        var user = await userManager.FindByNameAsync(req.Username.Trim());
        if (user is null)
            return await FallarAsync(null, "Los datos de acceso son incorrectos.");

        // RN-15
        if (user.Estado != EstadosUsuario.Activo)
            return await FallarAsync(user, "Usuario inactivo. Comuníquese con el administrador.");

        var result = await signInManager.CheckPasswordSignInAsync(user, req.Password, lockoutOnFailure: true);
        if (result.IsLockedOut)
            return await FallarAsync(user, "Cuenta bloqueada temporalmente por intentos fallidos.");
        if (!result.Succeeded)
            return await FallarAsync(user, "Los datos de acceso son incorrectos.");

        var roles = await userManager.GetRolesAsync(user);
        var (token, expira) = jwtService.Generate(user, roles);
        await audit.RegistrarAsync("LOGIN", "Usuario", null, ResultadoAuditoria.Exitoso, user.Id);

        return new AuthResponse
        {
            Success = true,
            Token = token,
            Expira = expira,
            UserId = user.Id,
            Username = user.UserName,
            NombreCompleto = $"{user.Nombre} {user.Apellido}",
            Roles = roles
        };
    }

    private async Task<AuthResponse> FallarAsync(ApplicationUser? user, string mensaje)
    {
        await audit.RegistrarAsync("LOGIN", "Usuario", null, ResultadoAuditoria.Fallido, user?.Id);
        return AuthResponse.Fail(mensaje);
    }
}
