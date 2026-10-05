using System.Text.Json;
using Bomberos.Integration.Api.Clients;
using Bomberos.Integration.Api.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bomberos.Integration.Api.Controllers;

/// <summary>
/// Paso directo: registra la petición y la reenvía a CORE sin cambios (CRUD y consultas generales).
/// Las rutas con lógica propia tienen prioridad sobre esta ruta comodín porque tienen segmentos fijos.
/// </summary>
[ApiController, Authorize]
[Route("api/v1/{**path}")]
[ApiExplorerSettings(IgnoreApi = true)]   // la ruta comodín no se puede describir en Swagger; se explica en la descripción del documento
public class PassthroughController(ICoreClient core) : ControllerBase
{
    [AcceptVerbs("GET", "POST", "PUT", "PATCH", "DELETE")]
    public async Task<IActionResult> Forward(string path)
    {
        var body = await new StreamReader(Request.Body).ReadToEndAsync();
        var r = await core.SendAsync(new HttpMethod(Request.Method),
            $"api/v1/{path}{Request.QueryString}", body.Length > 0 ? body : null, HttpContext.Bearer());

        if (!r.Disponible)
            return StatusCode(StatusCodes.Status503ServiceUnavailable, Respuestas.Error(Respuestas.CoreNoDisponible));

        return new ContentResult { StatusCode = r.StatusCode, Content = r.Body, ContentType = "application/json" };
    }
}

[ApiController]
[Route("api/v1/auth")]
public class AuthController(ICoreClient core) : ControllerBase
{
    /// <summary>Autentica contra CORE y devuelve el JWT. Si CORE está caído no se pueden validar credenciales.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] JsonElement credenciales)
    {
        var r = await core.SendAsync(HttpMethod.Post, "api/v1/auth/login", credenciales.GetRawText(), null);
        if (!r.Disponible)
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new
            {
                success = false,
                message = "El sistema central no está disponible; no se pueden validar credenciales en este momento."
            });

        return new ContentResult { StatusCode = r.StatusCode, Content = r.Body, ContentType = "application/json" };
    }
}
