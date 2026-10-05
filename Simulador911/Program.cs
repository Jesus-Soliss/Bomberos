using System.Collections.Concurrent;
using Microsoft.OpenApi;

// Simulador del Centro 911: institución externa que reporta emergencias a Integración
// y recibe los avisos de cambio de estado. No referencia ningún proyecto de la solución.

const string CabeceraApiKey = "X-Api-Key";

// Nombre de la ventana de consola, para distinguirla al ejecutar los cuatro proyectos a la vez.
try { Console.Title = "Bomberos - Simulador 911"; } catch (IOException) { /* sin consola */ }

var builder = WebApplication.CreateBuilder(args);

var integracionUrl = builder.Configuration["Integracion:BaseUrl"]
    ?? throw new InvalidOperationException("Falta Integracion:BaseUrl en la configuración.");
var apiKey = builder.Configuration["Integracion:ApiKey"] ?? "";

builder.Services.AddHttpClient("integracion", c =>
{
    c.BaseAddress = new Uri(integracionUrl);
    c.Timeout = TimeSpan.FromSeconds(15);
    c.DefaultRequestHeaders.Add(CabeceraApiKey, apiKey);
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Bomberos - Simulador Centro 911", Version = "v1" }));

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

// Los avisos se guardan en memoria: se pierden al reiniciar el simulador.
var avisos = new ConcurrentQueue<AvisoEstado>();

// Envía un reporte a Integración (se prueba desde Swagger).
app.MapPost("/enviar-reporte", async (ReporteRequest rep, IHttpClientFactory f) =>
{
    var errores = Validar(rep);
    if (errores.Count > 0)
        return Results.Json(new { success = false, message = string.Join(" ", errores), errors = errores },
            statusCode: StatusCodes.Status400BadRequest);

    rep = rep with
    {
        Referencia = rep.Referencia.Trim(), Tipo = rep.Tipo.Trim().ToUpperInvariant(),
        Descripcion = rep.Descripcion.Trim(), Direccion = rep.Direccion.Trim(),
        Prioridad = rep.Prioridad.Trim().ToUpperInvariant()
    };

    try
    {
        var http = f.CreateClient("integracion");
        var res = await http.PostAsJsonAsync("integration/911/emergencias", rep);
        return Results.Content(await res.Content.ReadAsStringAsync(), "application/json", statusCode: (int)res.StatusCode);
    }
    catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
    {
        return Results.Json(new { success = false, message = "Integración no responde." }, statusCode: StatusCodes.Status503ServiceUnavailable);
    }
})
.WithSummary("Reporta una emergencia a Integración")
.WithDescription("Tipo: INCENDIO, ACCIDENTE, RESCATE, INCIDENTE_PELIGROSO u OTRO. Prioridad: BAJA, MEDIA, ALTA o CRITICA.");

// Consulta en Integración el estado de una emergencia reportada.
app.MapGet("/consultar-estado/{referencia}", async (string referencia, IHttpClientFactory f) =>
{
    try
    {
        var http = f.CreateClient("integracion");
        var res = await http.GetAsync($"integration/911/emergencias/{Uri.EscapeDataString(referencia)}");
        return Results.Content(await res.Content.ReadAsStringAsync(), "application/json", statusCode: (int)res.StatusCode);
    }
    catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
    {
        return Results.Json(new { success = false, message = "Integración no responde." }, statusCode: StatusCodes.Status503ServiceUnavailable);
    }
})
.WithSummary("Consulta el estado de una emergencia reportada");

// Recibe los avisos de cambio de estado desde Integración.
app.MapPost("/api/notificaciones", (AvisoEstado aviso, HttpRequest request) =>
{
    if (request.Headers[CabeceraApiKey] != apiKey)
        return Results.Json(new { success = false, message = "API key inválida." }, statusCode: StatusCodes.Status401Unauthorized);

    avisos.Enqueue(aviso);
    return Results.Ok();
})
.WithSummary("Recibe un aviso de cambio de estado (lo invoca Integración)");

app.MapGet("/api/notificaciones", () => avisos.Reverse())
    .WithSummary("Avisos de cambio de estado recibidos, del más reciente al más antiguo");

app.Run();

// El simulador valida su propio reporte antes de enviarlo, como lo haría la institución real.
static List<string> Validar(ReporteRequest rep)
{
    string[] tipos = ["INCENDIO", "ACCIDENTE", "RESCATE", "INCIDENTE_PELIGROSO", "OTRO"];
    string[] prioridades = ["BAJA", "MEDIA", "ALTA", "CRITICA"];
    var errores = new List<string>();

    void Texto(string campo, string? valor, int maximo)
    {
        if (string.IsNullOrWhiteSpace(valor)) errores.Add($"{campo} es requerida.");
        else if (valor.Trim().Length > maximo) errores.Add($"{campo} admite un máximo de {maximo} caracteres.");
    }

    Texto("La referencia", rep.Referencia, 50);
    if (!string.IsNullOrWhiteSpace(rep.Referencia) && rep.Referencia.Trim().Any(c => char.IsWhiteSpace(c) || c is '/' or '\\' or '?' or '#' or '%'))
        errores.Add("La referencia no admite espacios ni los caracteres / \\ ? # %.");

    if (string.IsNullOrWhiteSpace(rep.Tipo)) errores.Add("El tipo es requerido.");
    else if (!tipos.Contains(rep.Tipo.Trim(), StringComparer.OrdinalIgnoreCase))
        errores.Add($"El tipo '{rep.Tipo}' no es válido. Valores permitidos: {string.Join(", ", tipos)}.");

    if (string.IsNullOrWhiteSpace(rep.Prioridad)) errores.Add("La prioridad es requerida.");
    else if (!prioridades.Contains(rep.Prioridad.Trim(), StringComparer.OrdinalIgnoreCase))
        errores.Add($"La prioridad '{rep.Prioridad}' no es válida. Valores permitidos: {string.Join(", ", prioridades)}.");

    Texto("La descripción", rep.Descripcion, 500);
    Texto("La dirección", rep.Direccion, 200);
    return errores;
}

public record ReporteRequest(string Referencia, string Tipo, string Descripcion, string Direccion, string Prioridad);
public record AvisoEstado(string Referencia, string Estado, DateTime Fecha);
