using System.Text.Json;
using System.Text.Json.Nodes;
using Bomberos.Integration.Api.Common;
using Bomberos.Integration.Api.Data;

namespace Bomberos.Integration.Api.Middleware;

/// <summary>Guarda cada petición recibida y su respuesta en IntegrationLog. Nunca guarda contraseñas ni tokens.</summary>
public class IntegrationLogMiddleware(RequestDelegate next, ILogger<IntegrationLogMiddleware> logger)
{
    private const string Oculto = "[OCULTO]";
    private const int LongitudMaxima = 20_000;
    private static readonly string[] ClavesSensibles = ["password", "clave", "token"];

    public async Task InvokeAsync(HttpContext ctx, IntegrationContext db)
    {
        var es911 = ctx.Request.Path.StartsWithSegments("/integration");
        if (!es911 && !ctx.Request.Path.StartsWithSegments("/api"))
        {
            await next(ctx);
            return;
        }

        ctx.Request.EnableBuffering();
        var requestBody = await new StreamReader(ctx.Request.Body).ReadToEndAsync();
        ctx.Request.Body.Position = 0;

        var original = ctx.Response.Body;
        using var buffer = new MemoryStream();
        ctx.Response.Body = buffer;
        string responseBody;
        try
        {
            await next(ctx);
        }
        finally
        {
            buffer.Position = 0;
            responseBody = await new StreamReader(buffer).ReadToEndAsync();
            buffer.Position = 0;
            await buffer.CopyToAsync(original);
            ctx.Response.Body = original;
        }

        try
        {
            var esLogin = ctx.Request.Path.Value!.Contains("/auth/login", StringComparison.OrdinalIgnoreCase);
            var endpoint = ctx.Request.Path + ctx.Request.QueryString;
            var origen = es911 ? "CENTRAL_911" : ctx.User.FindFirst("name")?.Value ?? "WEBAPP";

            db.ChangeTracker.Clear();
            db.IntegrationLogs.Add(new IntegrationLog
            {
                FechaHora = DateTime.UtcNow,
                Origen = origen.Length > 50 ? origen[..50] : origen,
                Metodo = ctx.Request.Method,
                Endpoint = endpoint.Length > 300 ? endpoint[..300] : endpoint,
                Peticion = esLogin ? Oculto : Preparar(requestBody),
                CodigoRespuesta = ctx.Response.StatusCode,
                Respuesta = esLogin ? Oculto : Preparar(responseBody),
                AtendidoLocal = ctx.FueAtendidoLocal()
            });
            await db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // Un fallo al registrar el log no debe afectar la respuesta al canal.
            logger.LogError(ex, "No se pudo guardar el IntegrationLog de {Method} {Path}", ctx.Request.Method, ctx.Request.Path);
        }
    }

    private static string? Preparar(string cuerpo)
    {
        if (string.IsNullOrWhiteSpace(cuerpo)) return null;
        cuerpo = OcultarSensibles(cuerpo);
        return cuerpo.Length > LongitudMaxima ? cuerpo[..LongitudMaxima] + "...[truncado]" : cuerpo;
    }

    private static string OcultarSensibles(string cuerpo)
    {
        if (!ClavesSensibles.Any(c => cuerpo.Contains(c, StringComparison.OrdinalIgnoreCase))) return cuerpo;
        try
        {
            var nodo = JsonNode.Parse(cuerpo);
            Ocultar(nodo);
            return nodo?.ToJsonString() ?? cuerpo;
        }
        catch (JsonException)
        {
            return Oculto;   // menciona un dato sensible y no se puede analizar: no se guarda
        }
    }

    private static void Ocultar(JsonNode? nodo)
    {
        switch (nodo)
        {
            case JsonObject objeto:
                foreach (var propiedad in objeto.ToList())
                {
                    if (ClavesSensibles.Any(c => propiedad.Key.Contains(c, StringComparison.OrdinalIgnoreCase)))
                        objeto[propiedad.Key] = Oculto;
                    else
                        Ocultar(propiedad.Value);
                }
                break;
            case JsonArray arreglo:
                foreach (var elemento in arreglo) Ocultar(elemento);
                break;
        }
    }
}
