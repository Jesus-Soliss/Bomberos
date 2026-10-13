using AutoMapper;
using Bomberos.Core.Application.Common;
using Bomberos.Core.Application.DTOs.Recursos;
using Bomberos.Core.Application.ViewModels;
using Bomberos.Core.Domain.Enums;
using Bomberos.WebApp.Infrastructure;
using Bomberos.WebApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Bomberos.WebApp.Controllers;

// Listados: personal del sistema (Consulta los ve en solo lectura; Cliente no entra). Formularios: Administrador.

[Authorize(Policy = Politicas.Personal)]
public class EstacionesController(IntegracionApi api, IMapper mapper) : AppController
{
    private const string Ruta = "api/v1/estaciones";

    public async Task<IActionResult> Index()
    {
        var r = await api.GetAsync<List<EstacionDto>>(Ruta);
        if (!r.Ok) ViewData["Error"] = r.Message;
        return View(r.Data ?? []);
    }

    [Authorize(Roles = Roles.Administrador)]
    public IActionResult Crear() => View("Form", new EstacionViewModel());

    [HttpPost, Authorize(Roles = Roles.Administrador)]
    public async Task<IActionResult> Crear(EstacionViewModel vm)
    {
        if (!ModelState.IsValid || !Validar(await api.SendAsync(HttpMethod.Post, Ruta, mapper.Map<GuardarEstacionDto>(vm))))
            return View("Form", vm);
        Exito("Estación creada.");
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = Roles.Administrador)]
    public async Task<IActionResult> Editar(int id)
    {
        var r = await api.GetAsync<EstacionDto>($"{Ruta}/{id}");
        if (!r.Ok) { Fallo(r.Message); return RedirectToAction(nameof(Index)); }
        return View("Form", mapper.Map<EstacionViewModel>(r.Data));
    }

    [HttpPost, Authorize(Roles = Roles.Administrador)]
    public async Task<IActionResult> Editar(EstacionViewModel vm)
    {
        if (!ModelState.IsValid || !Validar(await api.SendAsync(HttpMethod.Put, $"{Ruta}/{vm.Id}", mapper.Map<GuardarEstacionDto>(vm))))
            return View("Form", vm);
        Exito("Estación actualizada.");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, Authorize(Roles = Roles.Administrador)]
    public async Task<IActionResult> CambiarEstado(int id, EstadoEstacion estado)
    {
        var r = await api.SendAsync(HttpMethod.Patch, $"{Ruta}/{id}/estado", new CambiarEstadoEstacionDto { Estado = estado });
        Informar(r, estado == EstadoEstacion.Activa ? "Estación activada." : "Estación inactivada.");
        return RedirectToAction(nameof(Index));
    }
}

[Authorize(Policy = Politicas.Personal)]
public class BomberosController(IntegracionApi api, IMapper mapper) : AppController
{
    private const string Ruta = "api/v1/bomberos";

    public async Task<IActionResult> Index()
    {
        var r = await api.GetAsync<List<BomberoDto>>(Ruta);
        if (!r.Ok) ViewData["Error"] = r.Message;
        return View(r.Data ?? []);
    }

    [Authorize(Roles = Roles.Administrador)]
    public async Task<IActionResult> Crear() => await FormAsync(new BomberoViewModel());

    [HttpPost, Authorize(Roles = Roles.Administrador)]
    public async Task<IActionResult> Crear(BomberoViewModel vm)
    {
        if (!ModelState.IsValid || !Validar(await api.SendAsync(HttpMethod.Post, Ruta, mapper.Map<GuardarBomberoDto>(vm))))
            return await FormAsync(vm);
        Exito("Bombero registrado.");
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = Roles.Administrador)]
    public async Task<IActionResult> Editar(int id)
    {
        var r = await api.GetAsync<BomberoDto>($"{Ruta}/{id}");
        if (!r.Ok) { Fallo(r.Message); return RedirectToAction(nameof(Index)); }
        return await FormAsync(mapper.Map<BomberoViewModel>(r.Data));
    }

    [HttpPost, Authorize(Roles = Roles.Administrador)]
    public async Task<IActionResult> Editar(BomberoViewModel vm)
    {
        if (!ModelState.IsValid || !Validar(await api.SendAsync(HttpMethod.Put, $"{Ruta}/{vm.Id}", mapper.Map<GuardarBomberoDto>(vm))))
            return await FormAsync(vm);
        Exito("Bombero actualizado.");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, Authorize(Roles = Roles.Administrador)]
    public async Task<IActionResult> CambiarEstado(int id, EstadoBombero estado)
    {
        var r = await api.SendAsync(HttpMethod.Patch, $"{Ruta}/{id}/estado", new CambiarEstadoBomberoDto { Estado = estado });
        Informar(r, estado == EstadoBombero.Activo ? "Bombero activado." : "Bombero inactivado.");
        return RedirectToAction(nameof(Index));
    }

    private async Task<IActionResult> FormAsync(BomberoViewModel vm)
    {
        ViewBag.Estaciones = await EstacionesActivas.CargarAsync(api, vm.EstacionId);
        return View("Form", vm);
    }
}

[Authorize(Policy = Politicas.Personal)]
public class VehiculosController(IntegracionApi api, IMapper mapper) : AppController
{
    private const string Ruta = "api/v1/vehiculos";

    public async Task<IActionResult> Index()
    {
        var r = await api.GetAsync<List<VehiculoDto>>(Ruta);
        if (!r.Ok) ViewData["Error"] = r.Message;
        return View(r.Data ?? []);
    }

    [Authorize(Roles = Roles.Administrador)]
    public async Task<IActionResult> Crear() => await FormAsync(new VehiculoViewModel());

    [HttpPost, Authorize(Roles = Roles.Administrador)]
    public async Task<IActionResult> Crear(VehiculoViewModel vm)
    {
        if (!ModelState.IsValid || !Validar(await api.SendAsync(HttpMethod.Post, Ruta, mapper.Map<GuardarVehiculoDto>(vm))))
            return await FormAsync(vm);
        Exito("Vehículo registrado.");
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = Roles.Administrador)]
    public async Task<IActionResult> Editar(int id)
    {
        var r = await api.GetAsync<VehiculoDto>($"{Ruta}/{id}");
        if (!r.Ok) { Fallo(r.Message); return RedirectToAction(nameof(Index)); }
        return await FormAsync(mapper.Map<VehiculoViewModel>(r.Data));
    }

    [HttpPost, Authorize(Roles = Roles.Administrador)]
    public async Task<IActionResult> Editar(VehiculoViewModel vm)
    {
        if (!ModelState.IsValid || !Validar(await api.SendAsync(HttpMethod.Put, $"{Ruta}/{vm.Id}", mapper.Map<GuardarVehiculoDto>(vm))))
            return await FormAsync(vm);
        Exito("Vehículo actualizado.");
        return RedirectToAction(nameof(Index));
    }

    // El operador también puede actualizar el estado de un vehículo.
    [HttpPost, Authorize(Roles = Roles.AdministradorOperador)]
    public async Task<IActionResult> CambiarEstado(int id, EstadoVehiculo estado)
    {
        var r = await api.SendAsync(HttpMethod.Patch, $"{Ruta}/{id}/estado", new CambiarEstadoVehiculoDto { Estado = estado });
        Informar(r, $"Estado del vehículo actualizado a {estado.Etiqueta().ToLowerInvariant()}.");
        return RedirectToAction(nameof(Index));
    }

    private async Task<IActionResult> FormAsync(VehiculoViewModel vm)
    {
        ViewBag.Estaciones = await EstacionesActivas.CargarAsync(api, vm.EstacionId);
        return View("Form", vm);
    }
}

internal static class EstacionesActivas
{
    /// <summary>Estaciones activas para el desplegable (más la actual del registro, aunque esté inactiva).</summary>
    public static async Task<List<SelectListItem>> CargarAsync(IntegracionApi api, int? actual)
    {
        var r = await api.GetAsync<List<EstacionDto>>("api/v1/estaciones");
        return (r.Data ?? [])
            .Where(e => e.Estado == EstadoEstacion.Activa || e.Id == actual)
            .Select(e => new SelectListItem(e.Nombre, e.Id.ToString()))
            .ToList();
    }
}
