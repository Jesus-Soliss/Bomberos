using System.Net.Http.Json;

namespace Bomberos.Integration.Api.Clients;

public interface ICliente911
{
    /// <summary>Avisa al Centro 911 que una emergencia reportada por ellos cambió de estado.</summary>
    Task<bool> NotificarEstadoAsync(string referencia, string estado);
}

public class Cliente911 : ICliente911
{
    private readonly HttpClient _http;
    private readonly ILogger<Cliente911> _logger;

    // BaseAddress y cabecera X-Api-Key en Program.cs
    public Cliente911(HttpClient http, ILogger<Cliente911> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<bool> NotificarEstadoAsync(string referencia, string estado)
    {
        try
        {
            using var res = await _http.PostAsJsonAsync("api/notificaciones",
                new { referencia, estado, fecha = DateTime.UtcNow });
            if (!res.IsSuccessStatusCode)
                _logger.LogWarning("El Centro 911 respondió {Status} al aviso de {Referencia}", (int)res.StatusCode, referencia);
            return res.IsSuccessStatusCode;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            // El aviso es informativo: si falla se registra, pero no se cancela la operación.
            _logger.LogWarning(ex, "No se pudo avisar al Centro 911 el cambio de estado de {Referencia}", referencia);
            return false;
        }
    }
}
