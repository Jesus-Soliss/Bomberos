using AutoMapper;
using Bomberos.Core.Application.Common;
using Bomberos.Core.Application.DTOs.Usuarios;
using Bomberos.Core.Application.ViewModels;
using Bomberos.WebApp.Infrastructure;
using Bomberos.WebApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Bomberos.WebApp.Controllers;

[Authorize(Roles = Roles.Administrador)]
public class UsuariosController(IntegracionApi api, IMapper mapper) : AppController
{
    private const string Ruta = "api/v1/usuarios";

    public async Task<IActionResult> Index()
    {
        var r = await api.GetAsync<List<UsuarioDto>>(Ruta);
        if (!r.Ok) ViewData["Error"] = r.Message;
        await CargarRolesAsync();
        return View(r.Data ?? []);
    }

    public async Task<IActionResult> Crear()
    {
        await CargarRolesAsync();
        return View(new UsuarioCrearViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> Crear(UsuarioCrearViewModel vm)
    {
        if (ModelState.IsValid && Validar(await api.SendAsync(HttpMethod.Post, Ruta, mapper.Map<CrearUsuarioDto>(vm))))
        {
            Exito($"Usuario {vm.Username} creado.");
            return RedirectToAction(nameof(Index));
        }
        await CargarRolesAsync();
        return View(vm);
    }

    public async Task<IActionResult> Editar(string id)
    {
        var r = await api.GetAsync<UsuarioDto>($"{Ruta}/{id}");
        if (!r.Ok) { Fallo(r.Message); return RedirectToAction(nameof(Index)); }
        return View(mapper.Map<UsuarioEditarViewModel>(r.Data));
    }

    [HttpPost]
    public async Task<IActionResult> Editar(UsuarioEditarViewModel vm)
    {
        if (ModelState.IsValid && Validar(await api.SendAsync(HttpMethod.Put, $"{Ruta}/{vm.Id}", mapper.Map<EditarUsuarioDto>(vm))))
        {
            Exito($"Usuario {vm.Username} actualizado.");
            return RedirectToAction(nameof(Index));
        }
        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> CambiarEstado(string id, string estado)
    {
        var r = await api.SendAsync(HttpMethod.Patch, $"{Ruta}/{id}/estado", new CambiarEstadoUsuarioDto { Estado = estado });
        Informar(r, estado == EstadosUsuario.Activo ? "Usuario activado." : "Usuario inactivado.");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> CambiarRol(string id, string rol)
    {
        var r = await api.SendAsync(HttpMethod.Patch, $"{Ruta}/{id}/rol", new CambiarRolDto { Rol = rol });
        Informar(r, "Perfil del usuario actualizado.");
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> RestablecerPassword(string id)
    {
        var r = await api.GetAsync<UsuarioDto>($"{Ruta}/{id}");
        if (!r.Ok) { Fallo(r.Message); return RedirectToAction(nameof(Index)); }
        return View(mapper.Map<RestablecerPasswordViewModel>(r.Data));
    }

    [HttpPost]
    public async Task<IActionResult> RestablecerPassword(RestablecerPasswordViewModel vm)
    {
        if (ModelState.IsValid && Validar(await api.SendAsync(HttpMethod.Patch, $"{Ruta}/{vm.Id}/password",
                new RestablecerPasswordDto { NuevaPassword = vm.NuevaPassword })))
        {
            Exito($"Contraseña de {vm.Username} restablecida.");
            return RedirectToAction(nameof(Index));
        }
        return View(vm);
    }

    private async Task CargarRolesAsync()
    {
        var roles = await api.GetAsync<List<RolDto>>("api/v1/roles");
        ViewBag.Roles = (roles.Data ?? []).Select(r => new SelectListItem(r.Nombre, r.Nombre)).ToList();
    }
}

/// <summary>Perfiles: son los roles de Identity. Se asignan a un usuario desde la pantalla de usuarios.</summary>
[Authorize(Roles = Roles.Administrador)]
public class RolesController(IntegracionApi api, IMapper mapper) : AppController
{
    private const string Ruta = "api/v1/roles";

    public async Task<IActionResult> Index()
    {
        var r = await api.GetAsync<List<RolDto>>(Ruta);
        if (!r.Ok) ViewData["Error"] = r.Message;
        return View(r.Data ?? []);
    }

    [HttpPost]
    public async Task<IActionResult> Crear(RolViewModel vm)
    {
        if (!ModelState.IsValid) Fallo("El nombre del perfil es requerido (máximo 50 caracteres).");
        else Informar(await api.SendAsync(HttpMethod.Post, Ruta, mapper.Map<GuardarRolDto>(vm)), "Perfil creado.");
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Editar(string id)
    {
        var r = await api.GetAsync<List<RolDto>>(Ruta);
        var rol = r.Data?.FirstOrDefault(x => x.Id == id);
        if (rol is null) { Fallo(r.Ok ? "Perfil no encontrado." : r.Message); return RedirectToAction(nameof(Index)); }
        return View(mapper.Map<RolViewModel>(rol));
    }

    [HttpPost]
    public async Task<IActionResult> Editar(RolViewModel vm)
    {
        if (ModelState.IsValid && Validar(await api.SendAsync(HttpMethod.Put, $"{Ruta}/{vm.Id}", mapper.Map<GuardarRolDto>(vm))))
        {
            Exito("Perfil actualizado.");
            return RedirectToAction(nameof(Index));
        }
        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> Eliminar(string id)
    {
        Informar(await api.SendAsync(HttpMethod.Delete, $"{Ruta}/{id}"), "Perfil eliminado.");
        return RedirectToAction(nameof(Index));
    }
}
