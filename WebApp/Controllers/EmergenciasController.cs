using AutoMapper;
using Bomberos.Core.Application.Common;
using Bomberos.Core.Application.DTOs.Emergencias;
using Bomberos.Core.Application.ViewModels;
using Bomberos.Core.Domain.Common;
using Bomberos.Core.Domain.Enums;
using Bomberos.WebApp.Infrastructure;
using Bomberos.WebApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bomberos.WebApp.Controllers;

// Listado y detalle: cualquier usuario autenticado. Registro, estados y asignaciones: Administrador y Operador.
[Authorize]
public class EmergenciasController(IntegracionApi api, IMapper mapper) : AppController
{
    private const string Ruta = "api/v1/emergencias";

    public async Task<IActionResult> Index(EmergenciaFiltroViewModel filtro)
    {
        var r = await api.GetAsync<List<EmergenciaDto>>(Ruta + QueryDe(filtro));
        if (!r.Ok) ViewData["Error"] = r.Message;
        return View(new EmergenciasIndexViewModel
        {
            Filtro = filtro,
            Emergencias = r.Data ?? [],
            Contingencia = r.NoDisponible
        });
    }

    public async Task<IActionResult> Detalle(int id)
    {
        var r = await api.GetAsync<EmergenciaDetalleDto>($"{Ruta}/{id}");
        if (!r.Ok && !r.NoDisponible)
        {
            Fallo(r.Message);
            return RedirectToAction(nameof(Index));
        }

        // Con CORE caído no hay detalle, pero Integración acepta asignaciones con su copia local.
        var vm = new EmergenciaDetalleViewModel { Id = id, Emergencia = r.Data };
        if (User.PuedeOperar() && (vm.Contingencia || vm.Emergencia!.Estado is EstadoEmergencia.Pendiente or EstadoEmergencia.EnAtencion))
        {
            var bomberos = await api.GetAsync<List<RecursoDisponibleViewModel>>("api/v1/bomberos/disponibles");
            var vehiculos = await api.GetAsync<List<RecursoDisponibleViewModel>>("api/v1/vehiculos/disponibles");
            vm.BomberosDisponibles = bomberos.Data ?? [];
            vm.VehiculosDisponibles = vehiculos.Data ?? [];
            vm.DisponiblesDeCopiaLocal = bomberos.AtendidoLocal || vehiculos.AtendidoLocal;
        }
        return View(vm);
    }

    [Authorize(Roles = Roles.AdministradorOperador)]
    public IActionResult Crear() => View("Form", new EmergenciaViewModel());

    [HttpPost, Authorize(Roles = Roles.AdministradorOperador)]
    public async Task<IActionResult> Crear(EmergenciaViewModel vm)
    {
        if (!ModelState.IsValid) return View("Form", vm);

        var r = await api.SendAsync<EmergenciaDto>(HttpMethod.Post, Ruta, mapper.Map<GuardarEmergenciaDto>(vm));
        if (!Validar(r)) return View("Form", vm);

        if (r.Pendiente)   // CORE caído: quedó en cola
        {
            Aviso(r.Message!);
            return RedirectToAction(nameof(Index));
        }
        Exito("Emergencia registrada.");
        return RedirectToAction(nameof(Detalle), new { id = r.Data!.Id });
    }

    [Authorize(Roles = Roles.AdministradorOperador)]
    public async Task<IActionResult> Editar(int id)
    {
        var r = await api.GetAsync<EmergenciaDetalleDto>($"{Ruta}/{id}");
        if (!r.Ok) { Fallo(r.Message); return RedirectToAction(nameof(Index)); }
        return View("Form", mapper.Map<EmergenciaViewModel>(r.Data));
    }

    [HttpPost, Authorize(Roles = Roles.AdministradorOperador)]
    public async Task<IActionResult> Editar(EmergenciaViewModel vm)
    {
        if (!ModelState.IsValid || !Validar(await api.SendAsync(HttpMethod.Put, $"{Ruta}/{vm.Id}", mapper.Map<GuardarEmergenciaDto>(vm))))
            return View("Form", vm);
        Exito("Emergencia actualizada.");
        return RedirectToAction(nameof(Detalle), new { id = vm.Id });
    }

    [HttpPost, Authorize(Roles = Roles.AdministradorOperador)]
    public async Task<IActionResult> CambiarEstado(int id, EstadoEmergencia estado)
    {
        var r = await api.SendAsync(HttpMethod.Patch, $"{Ruta}/{id}/estado", new CambiarEstadoEmergenciaDto { Estado = estado });
        Informar(r, $"La emergencia pasó a {estado.Etiqueta().ToLowerInvariant()}.");
        return RedirectToAction(nameof(Detalle), new { id });
    }

    [HttpPost, Authorize(Roles = Roles.AdministradorOperador)]
    public async Task<IActionResult> AsignarBombero(int id, int bomberoId)
    {
        if (bomberoId <= 0) Fallo("Seleccione un bombero.");
        else Informar(await api.SendAsync(HttpMethod.Post, $"{Ruta}/{id}/bomberos", new AsignarBomberoDto { BomberoId = bomberoId }), "Bombero asignado.");
        return RedirectToAction(nameof(Detalle), new { id });
    }

    [HttpPost, Authorize(Roles = Roles.AdministradorOperador)]
    public async Task<IActionResult> AsignarVehiculo(int id, int vehiculoId)
    {
        if (vehiculoId <= 0) Fallo("Seleccione un vehículo.");
        else Informar(await api.SendAsync(HttpMethod.Post, $"{Ruta}/{id}/vehiculos", new AsignarVehiculoDto { VehiculoId = vehiculoId }), "Vehículo asignado.");
        return RedirectToAction(nameof(Detalle), new { id });
    }

    /// <param name="recurso">bomberos o vehiculos</param>
    /// <param name="accion">finalizar o cancelar</param>
    [HttpPost, Authorize(Roles = Roles.AdministradorOperador)]
    public async Task<IActionResult> CerrarAsignacion(int id, string recurso, int recursoId, string accion)
    {
        if (recurso is not ("bomberos" or "vehiculos") || accion is not ("finalizar" or "cancelar")) return BadRequest();

        var r = await api.SendAsync(HttpMethod.Patch, $"{Ruta}/{id}/{recurso}/{recursoId}/{accion}");
        Informar(r, accion == "finalizar" ? "Asignación finalizada." : "Asignación cancelada.");
        return RedirectToAction(nameof(Detalle), new { id });
    }

    private static string QueryDe(EmergenciaFiltroViewModel f)
    {
        var partes = new List<string>();
        if (f.Estado is { } estado) partes.Add($"estado={estado.ToTexto()}");
        if (f.Tipo is { } tipo) partes.Add($"tipo={tipo.ToTexto()}");
        if (f.Prioridad is { } prioridad) partes.Add($"prioridad={prioridad.ToTexto()}");
        if (f.Origen is { } origen) partes.Add($"origen={origen.ToTexto()}");
        partes.AddRange(RangoFechas.Query(f.Desde, f.Hasta));
        return partes.Count == 0 ? "" : "?" + string.Join("&", partes);
    }
}

internal static class RangoFechas
{
    /// <summary>El usuario filtra por días en hora local; la API compara contra fechas en UTC.</summary>
    public static IEnumerable<string> Query(DateTime? desde, DateTime? hasta)
    {
        if (desde is { } d) yield return $"desde={Utc(d.Date)}";
        if (hasta is { } h) yield return $"hasta={Utc(h.Date.AddDays(1).AddSeconds(-1))}";   // hasta el final del día
    }

    private static string Utc(DateTime local)
        => Uri.EscapeDataString(DateTime.SpecifyKind(local, DateTimeKind.Local).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss"));
}
