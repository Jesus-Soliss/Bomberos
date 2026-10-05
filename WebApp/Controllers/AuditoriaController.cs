using Bomberos.Core.Application.Common;
using Bomberos.Core.Application.DTOs.Consultas;
using Bomberos.Core.Application.DTOs.Usuarios;
using Bomberos.Core.Application.ViewModels;
using Bomberos.WebApp.Infrastructure;
using Bomberos.WebApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Bomberos.WebApp.Controllers;

[Authorize(Roles = Roles.Administrador)]
public class AuditoriaController(IntegracionApi api) : AppController
{
    /// <summary>Rastro de acciones de los usuarios (AuditLog).</summary>
    public async Task<IActionResult> Index(LogFiltroViewModel filtro)
    {
        var r = await api.GetAsync<List<AuditLogDto>>("api/v1/auditoria" + QueryDe(filtro));
        if (!r.Ok) ViewData["Error"] = r.Message;
        await CargarUsuariosAsync(filtro.UsuarioId);
        ViewBag.Filtro = filtro;
        return View(r.Data ?? []);
    }

    /// <summary>Peticiones y respuestas recibidas por la WebApi de CORE (RequestLog).</summary>
    public async Task<IActionResult> Peticiones(LogFiltroViewModel filtro)
    {
        var r = await api.GetAsync<List<RequestLogDto>>("api/v1/request-logs" + QueryDe(filtro));
        if (!r.Ok) ViewData["Error"] = r.Message;
        await CargarUsuariosAsync(filtro.UsuarioId);
        ViewBag.Filtro = filtro;
        return View(r.Data ?? []);
    }

    /// <summary>Operaciones que Integración recibió con CORE caído: pendientes, aplicadas y rechazadas con su motivo.</summary>
    public async Task<IActionResult> Pendientes(string? estado)
    {
        var query = string.IsNullOrWhiteSpace(estado) ? "" : $"?estado={Uri.EscapeDataString(estado)}";
        var r = await api.GetAsync<List<OperacionPendienteViewModel>>("api/v1/integracion/pendientes" + query);
        if (!r.Ok) ViewData["Error"] = r.Message;
        await CargarUsuariosAsync(null);
        ViewBag.Estado = estado;
        return View(r.Data ?? []);
    }

    // Los logs guardan el Id del usuario; en pantalla se muestra su nombre.
    private async Task CargarUsuariosAsync(string? seleccionado)
    {
        var usuarios = (await api.GetAsync<List<UsuarioDto>>("api/v1/usuarios")).Data ?? [];
        ViewBag.Usuarios = usuarios.ToDictionary(u => u.Id, u => u.Username);
        ViewBag.UsuariosLista = usuarios
            .Select(u => new SelectListItem(u.Username, u.Id, u.Id == seleccionado))
            .ToList();
    }

    private static string QueryDe(LogFiltroViewModel f)
    {
        var partes = new List<string> { "limite=300" };
        if (!string.IsNullOrWhiteSpace(f.UsuarioId)) partes.Add($"usuarioId={Uri.EscapeDataString(f.UsuarioId)}");
        if (!string.IsNullOrWhiteSpace(f.Accion)) partes.Add($"accion={Uri.EscapeDataString(f.Accion.Trim())}");
        partes.AddRange(RangoFechas.Query(f.Desde, f.Hasta));
        return "?" + string.Join("&", partes);
    }
}
