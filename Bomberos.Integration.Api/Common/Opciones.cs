namespace Bomberos.Integration.Api.Common;

public class CoreOptions
{
    public string BaseUrl { get; set; } = "";
    /// <summary>Cuenta de servicio (rol OPERADOR) creada en el seed de CORE.</summary>
    public string ServiceUser { get; set; } = "";
    public string ServicePassword { get; set; } = "";
    public int TimeoutSegundos { get; set; } = 5;
}

public class Centro911Options
{
    public string BaseUrl { get; set; } = "";
    public string ApiKey { get; set; } = "";
}

public class SyncOptions
{
    public double IntervaloMinutos { get; set; } = 2;
    public int MaxReintentos { get; set; } = 5;
}

public class JwtOptions
{
    public string Key { get; set; } = "";
    public string Issuer { get; set; } = "";
    public string Audience { get; set; } = "";
}

public static class Respuestas
{
    public const string CoreNoDisponible = "El sistema central no está disponible.";
    public const string OperacionEncolada = "Operación recibida. Se aplicará cuando el sistema central se restablezca.";

    public static object Error(string message) => new { success = false, message };
}

public static class HttpContextExtensions
{
    private const string ClaveAtendidoLocal = "AtendidoLocal";
    public const string CabeceraAtendidoLocal = "X-Atendido-Local";

    /// <summary>Marca la respuesta como generada con la copia local (CORE caído).</summary>
    public static void MarcarAtendidoLocal(this HttpContext ctx)
    {
        ctx.Items[ClaveAtendidoLocal] = true;
        ctx.Response.Headers[CabeceraAtendidoLocal] = "true";
    }

    public static bool FueAtendidoLocal(this HttpContext ctx) => ctx.Items.ContainsKey(ClaveAtendidoLocal);

    public static string? Bearer(this HttpContext ctx)
    {
        var valor = ctx.Request.Headers.Authorization.ToString();
        return valor.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) ? valor["Bearer ".Length..].Trim() : null;
    }
}
