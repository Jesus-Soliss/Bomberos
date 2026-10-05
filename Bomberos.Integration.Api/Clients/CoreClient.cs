using System.Net.Http.Headers;
using System.Text;

namespace Bomberos.Integration.Api.Clients;

/// <param name="Disponible">false si CORE está caído o no respondió a tiempo.</param>
public record CoreResult(bool Disponible, int StatusCode, string? Body)
{
    public bool Exitoso => Disponible && StatusCode is >= 200 and < 300;
}

public interface ICoreClient
{
    Task<CoreResult> SendAsync(HttpMethod method, string path, string? json, string? bearer);
}

public class CoreClient : ICoreClient
{
    private readonly HttpClient _http;

    public CoreClient(HttpClient http) => _http = http;   // BaseAddress y Timeout en Program.cs

    public async Task<CoreResult> SendAsync(HttpMethod method, string path, string? json, string? bearer)
    {
        try
        {
            using var req = new HttpRequestMessage(method, path);
            if (json is not null)
                req.Content = new StringContent(json, Encoding.UTF8, "application/json");
            if (bearer is not null)
                req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearer);

            using var res = await _http.SendAsync(req);
            return new CoreResult(true, (int)res.StatusCode, await res.Content.ReadAsStringAsync());
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            return new CoreResult(false, 0, null);   // CORE caído o sin respuesta
        }
    }
}
