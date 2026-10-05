using System.Diagnostics;
using Bomberos.Core.Application.DTOs.Consultas;
using Bomberos.WebApp.Infrastructure;
using Bomberos.WebApp.Models;
using Bomberos.WebApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bomberos.WebApp.Controllers;

[Authorize]
public class HomeController(IntegracionApi api) : AppController
{
    /// <summary>Dashboard: el mismo para los tres roles; el menú cambia según el perfil.</summary>
    public async Task<IActionResult> Index()
    {
        var r = await api.GetAsync<DashboardDto>("api/v1/dashboard");
        if (!r.Ok) ViewData["Error"] = r.Message;
        return View(r.Data);
    }

    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
        => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
