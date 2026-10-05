using System.Collections.Concurrent;
using Microsoft.OpenApi;

// Simulador del Centro 911: institución externa que reporta emergencias a Integración
// y recibe los avisos de cambio de estado. No referencia ningún proyecto de la solución.

const string CabeceraApiKey = "X-Api-Key";

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

public record ReporteRequest(string Referencia, string Tipo, string Descripcion, string Direccion, string Prioridad);
public record AvisoEstado(string Referencia, string Estado, DateTime Fecha);
