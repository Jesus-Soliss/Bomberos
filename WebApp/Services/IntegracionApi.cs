using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Bomberos.Core.Application.Common;
using Microsoft.AspNetCore.Authentication;

namespace Bomberos.WebApp.Services;

public class ApiResult<T>
{
    public int Status { get; init; }
    public T? Data { get; init; }
    public string? Message { get; init; }
    /// <summary>Integración respondió con su copia local porque CORE está caído.</summary>
    public bool AtendidoLocal { get; init; }

    public bool Ok => Status is >= 200 and < 300;
    /// <summary>202: Integración recibió la operación y la aplicará cuando CORE se restablezca.</summary>
    public bool Pendiente => Status == StatusCodes.Status202Accepted;
    /// <summary>CORE (503) o la propia Integración (0) no responden.</summary>
    public bool NoDisponible => Status is 0 or StatusCodes.Status503ServiceUnavailable;
    public bool NoEncontrado => Status == StatusCodes.Status404NotFound;
}

/// <summary>El JWT venció o ya no es válido: hay que volver a iniciar sesión.</summary>
public class SesionExpiradaException : Exception;

/// <summary>La API rechazó la operación por falta de permisos (403).</summary>
public class AccesoDenegadoException : Exception;

/// <summary>Único punto de salida de la WebApp: todo pasa por Integration.Api, nunca directo a CORE.</summary>
public class IntegracionApi(IHttpClientFactory factory, ILogger<IntegracionApi> logger)
{
    public const string Cliente = "integracion";
    public const string MensajeNoDisponible = "El sistema central no está disponible. Intente de nuevo en unos minutos.";
    public const string MensajePendiente = "La operación fue recibida y se aplicará cuando el sistema central se restablezca.";

    public Task<ApiResult<T>> GetAsync<T>(string path) => SendAsync<T>(HttpMethod.Get, path);

    public Task<ApiResult<object>> SendAsync(HttpMethod method, string path, object? body = null)
        => SendAsync<object>(method, path, body);

    /// <param name="anonimo">Llamada sin sesión (login): un 401 es una respuesta normal, no una sesión vencida.</param>
    public async Task<ApiResult<T>> SendAsync<T>(HttpMethod method, string path, object? body = null, bool anonimo = false)
    {
        HttpResponseMessage res;
        try
        {
            using var req = new HttpRequestMessage(method, path);
            if (body is not null) req.Content = JsonContent.Create(body, body.GetType(), options: ApiJson.Options);
            res = await factory.CreateClient(Cliente).SendAsync(req);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            logger.LogWarning(ex, "Integración no responde en {Method} {Path}", method, path);
            return new ApiResult<T> { Status = 0, Message = "El servicio de integración no responde. Intente de nuevo en unos minutos." };
        }

        using (res)
        {
            if (res.StatusCode == HttpStatusCode.Unauthorized && !anonimo) throw new SesionExpiradaException();
            if (res.StatusCode == HttpStatusCode.Forbidden) throw new AccesoDenegadoException();

            var status = (int)res.StatusCode;
            var local = res.Headers.TryGetValues("X-Atendido-Local", out var v) && v.Contains("true");
            var contenido = await res.Content.ReadAsStringAsync();

            if (status == StatusCodes.Status202Accepted)
                return new ApiResult<T> { Status = status, Message = MensajePendiente, AtendidoLocal = true };

            if (res.IsSuccessStatusCode)
                return new ApiResult<T>
                {
                    Status = status,
                    AtendidoLocal = local,
                    Data = string.IsNullOrWhiteSpace(contenido) ? default : JsonSerializer.Deserialize<T>(contenido, ApiJson.Options)
                };

            return new ApiResult<T>
            {
                Status = status,
                Message = status == StatusCodes.Status503ServiceUnavailable
                    ? MensajeNoDisponible
                    : LeerMensaje(contenido) ?? $"La operación no se pudo completar (código {status})."
            };
        }
    }

    // Los errores llegan como { "success": false, "message": "..." }.
    private static string? LeerMensaje(string contenido)
    {
        if (string.IsNullOrWhiteSpace(contenido)) return null;
        try
        {
            using var doc = JsonDocument.Parse(contenido);
            return doc.RootElement.ValueKind == JsonValueKind.Object
                   && doc.RootElement.TryGetProperty("message", out var m)
                   && m.ValueKind == JsonValueKind.String ? m.GetString() : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }
}

/// <summary>Agrega a cada llamada el JWT guardado en la cookie de sesión.</summary>
public class BearerTokenHandler(IHttpContextAccessor accessor) : DelegatingHandler
{
    public const string NombreToken = "access_token";

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage req, CancellationToken ct)
    {
        var ctx = accessor.HttpContext;
        var token = ctx is null ? null : await ctx.GetTokenAsync(NombreToken);
        if (token is not null)
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await base.SendAsync(req, ct);
    }
}
