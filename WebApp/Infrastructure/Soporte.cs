using System.Security.Claims;
using Bomberos.Core.Application.Common;
using Bomberos.Core.Domain.Common;
using Bomberos.WebApp.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Bomberos.WebApp.Infrastructure;

/// <summary>Base de los controllers: mensajes al usuario según la respuesta de la API.</summary>
public abstract class AppController : Controller
{
    protected void Exito(string mensaje) => TempData["Exito"] = mensaje;
    protected void Aviso(string mensaje) => TempData["Aviso"] = mensaje;
    protected void Fallo(string? mensaje) => TempData["Error"] = mensaje ?? "La operación no se pudo completar.";

    /// <summary>Muestra el resultado de una operación: éxito, pendiente (202) o el mensaje de la regla de negocio.</summary>
    protected bool Informar<T>(ApiResult<T> r, string exito)
    {
        if (r.Pendiente) Aviso(r.Message!);
        else if (r.Ok) Exito(exito);
        else Fallo(r.Message);
        return r.Ok;
    }

    /// <summary>Para formularios: deja el error de la API en el ModelState y devuelve false si falló.</summary>
    protected bool Validar<T>(ApiResult<T> r)
    {
        if (r.Ok) return true;
        ModelState.AddModelError(string.Empty, r.Message ?? "La operación no se pudo completar.");
        return false;
    }
}

/// <summary>Sesión vencida → Login. Sin permiso en la API → Acceso denegado.</summary>
public class ApiExceptionFilter(ITempDataDictionaryFactory tempData) : IAsyncExceptionFilter
{
    public async Task OnExceptionAsync(ExceptionContext context)
    {
        switch (context.Exception)
        {
            case SesionExpiradaException:
                await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                tempData.GetTempData(context.HttpContext)["Aviso"] = "Su sesión expiró. Inicie sesión nuevamente.";
                context.Result = new RedirectToActionResult("Login", "Account", null);
                context.ExceptionHandled = true;
                break;
            case AccesoDenegadoException:
                context.Result = new RedirectToActionResult("AccesoDenegado", "Account", null);
                context.ExceptionHandled = true;
                break;
        }
    }
}

public static class UsuarioExtensions
{
    public static bool EsAdministrador(this ClaimsPrincipal user) => user.IsInRole(Roles.Administrador);
    /// <summary>Administrador u operador: puede gestionar emergencias y asignaciones.</summary>
    public static bool PuedeOperar(this ClaimsPrincipal user) => user.IsInRole(Roles.Administrador) || user.IsInRole(Roles.Operador);
    /// <summary>Cliente: solo puede registrar emergencias.</summary>
    public static bool EsCliente(this ClaimsPrincipal user) => user.IsInRole(Roles.Cliente);
    public static string NombreCompleto(this ClaimsPrincipal user) => user.FindFirst("nombre")?.Value ?? user.Identity?.Name ?? "";
    public static string RolPrincipal(this ClaimsPrincipal user) => user.FindFirst("role")?.Value ?? "";
}

public static class Formato
{
    /// <summary>La API guarda y devuelve las fechas en UTC; en pantalla se muestran en hora local.</summary>
    public static string Fecha(this DateTime fecha)
        => DateTime.SpecifyKind(fecha, DateTimeKind.Utc).ToLocalTime().ToString("dd/MM/yyyy HH:mm");

    public static string Fecha(this DateTime? fecha) => fecha is { } f ? f.Fecha() : "—";

    /// <summary>EN_ATENCION → "En atencion", CENTRAL_911 → "Central 911".</summary>
    public static string Etiqueta(string texto)
    {
        if (string.IsNullOrEmpty(texto)) return "";
        var limpio = texto.Replace('_', ' ').ToLowerInvariant();
        return char.ToUpperInvariant(limpio[0]) + limpio[1..];
    }

    public static string Etiqueta<TEnum>(this TEnum valor) where TEnum : struct, Enum => Etiqueta(valor.ToTexto());

    public static List<SelectListItem> Opciones<TEnum>() where TEnum : struct, Enum
        => Enum.GetValues<TEnum>().Select(e => new SelectListItem(e.Etiqueta(), e.ToString())).ToList();

    public static IHtmlContent Estado<TEnum>(this IHtmlHelper html, TEnum valor) where TEnum : struct, Enum
        => html.Estado(valor.ToTexto());

    /// <summary>Etiqueta de color para estados, prioridades y resultados.</summary>
    public static IHtmlContent Estado(this IHtmlHelper html, string? texto)
    {
        texto ??= "";
        var color = texto switch
        {
            "ACTIVO" or "ACTIVA" or "DISPONIBLE" or "FINALIZADA" or "EXITOSO" or "APLICADA" or "BAJA" => "success",
            "EN_ATENCION" or "EN_SERVICIO" or "OPERADOR" or "MEDIA" => "primary",
            "CONTROLADA" or "CENTRAL_911" => "info",
            "PENDIENTE" or "MANTENIMIENTO" or "RESERVADO" or "ALTA" => "warning",
            "FUERA_DE_SERVICIO" or "FALLIDO" or "RECHAZADA" or "CRITICA" => "danger",
            _ => "secondary"
        };
        var claseTexto = color is "warning" or "info" ? " text-dark" : "";
        return new HtmlString($"<span class=\"badge bg-{color}{claseTexto}\">{html.Encode(Etiqueta(texto))}</span>");
    }
}
