using System.Security.Claims;
using AutoMapper;
using Bomberos.Core.Application.DTOs.Auth;
using Bomberos.Core.Application.ViewModels;
using Bomberos.WebApp.Infrastructure;
using Bomberos.WebApp.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bomberos.WebApp.Controllers;

public class AccountController(IntegracionApi api, IMapper mapper) : AppController
{
    [HttpGet, AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true) return RedirectToAction("Index", "Home");

        // Llegó aquí redirigido desde una sección protegida.
        if (!string.IsNullOrEmpty(returnUrl) && returnUrl != "/")
            ViewData["Aviso"] = "No tiene permiso para acceder a esa sección. Inicie sesión para continuar.";
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost, AllowAnonymous]
    public async Task<IActionResult> Login(LoginViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        // Integración -> CORE
        var r = await api.SendAsync<AuthResponse>(HttpMethod.Post, "api/v1/auth/login", mapper.Map<AuthRequest>(vm), anonimo: true);
        if (!r.Ok || r.Data?.Token is null)
        {
            // Usuario inactivo, cuenta bloqueada o credenciales incorrectas: el mensaje lo decide CORE.
            ModelState.AddModelError(string.Empty, r.Message ?? "No se pudo iniciar sesión.");
            return View(vm);
        }

        var auth = r.Data;
        var claims = new List<Claim>
        {
            new("sub", auth.UserId ?? ""),
            new("name", auth.Username ?? vm.Username),
            new("nombre", auth.NombreCompleto ?? "")
        };
        claims.AddRange(auth.Roles.Select(rol => new Claim("role", rol)));
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme,
            nameType: "name", roleType: "role");

        // La sesión dura lo mismo que el JWT, que se guarda dentro de la cookie.
        var props = new AuthenticationProperties
        {
            ExpiresUtc = auth.Expira is { } expira ? DateTime.SpecifyKind(expira, DateTimeKind.Utc) : null
        };
        props.StoreTokens([new AuthenticationToken { Name = BearerTokenHandler.NombreToken, Value = auth.Token }]);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity), props);

        return Url.IsLocalUrl(vm.ReturnUrl) ? Redirect(vm.ReturnUrl!) : RedirectToAction("Index", "Home");
    }

    [HttpGet, AllowAnonymous]
    public IActionResult Registro()
        => User.Identity?.IsAuthenticated == true ? RedirectToAction("Index", "Home") : View(new RegistroViewModel());

    /// <summary>Registro público: la cuenta siempre queda con el perfil Cliente (lo decide CORE).</summary>
    [HttpPost, AllowAnonymous]
    public async Task<IActionResult> Registro(RegistroViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var r = await api.SendAsync<object>(HttpMethod.Post, "api/v1/auth/registro", mapper.Map<RegistroClienteDto>(vm), anonimo: true);
        if (!Validar(r)) return View(vm);

        Exito("Cuenta creada. Inicie sesión para registrar una emergencia.");
        return RedirectToAction(nameof(Login));
    }

    [HttpPost, Authorize]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    [HttpGet, Authorize]
    public IActionResult AccesoDenegado() => View();
}
