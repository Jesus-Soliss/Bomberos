using Bomberos.Core.Application.Common;
using Bomberos.Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Bomberos.Infrastructure.Identity.Seeds;

public static class IdentitySeeder
{
    public static Task SeedAsync(IServiceProvider services) => SeedAsync(
        services.GetRequiredService<UserManager<ApplicationUser>>(),
        services.GetRequiredService<RoleManager<IdentityRole>>());

    // Las contraseñas de ejemplo son solo para desarrollo.
    public static async Task SeedAsync(UserManager<ApplicationUser> users, RoleManager<IdentityRole> roles)
    {
        foreach (var r in Roles.Base)
            if (!await roles.RoleExistsAsync(r)) await roles.CreateAsync(new IdentityRole(r));

        await CrearUsuario(users, "admin", "Admin", "Sistema", Roles.Administrador, "Admin123*");
        await CrearUsuario(users, "operador", "Operador", "Demo", Roles.Operador, "Operador123*");
        await CrearUsuario(users, "consulta", "Consulta", "Demo", Roles.Consulta, "Consulta123*");
        // Cuenta de servicio que usa Integration.Api para sincronizar.
        await CrearUsuario(users, "svc.integracion", "Servicio", "Integracion", Roles.Operador, "Svc.Integr4cion*");
    }

    private static async Task CrearUsuario(UserManager<ApplicationUser> users, string username, string nombre,
        string apellido, string rol, string password)
    {
        if (await users.FindByNameAsync(username) is not null) return;

        var user = new ApplicationUser
        {
            UserName = username,
            Email = $"{username}@bomberos.local",
            EmailConfirmed = true,
            Nombre = nombre,
            Apellido = apellido,
            Estado = EstadosUsuario.Activo
        };

        var result = await users.CreateAsync(user, password);
        if (!result.Succeeded)
            throw new InvalidOperationException(
                $"No se pudo crear el usuario '{username}': {string.Join(" ", result.Errors.Select(e => e.Description))}");

        await users.AddToRoleAsync(user, rol);
    }
}
