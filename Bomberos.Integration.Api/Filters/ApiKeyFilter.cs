using Bomberos.Integration.Api.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;

namespace Bomberos.Integration.Api.Filters;

/// <summary>Seguridad del Centro 911: compara la cabecera X-Api-Key con la clave configurada.</summary>
public class ApiKeyFilter(IOptions<Centro911Options> options) : IAsyncActionFilter
{
    public const string Cabecera = "X-Api-Key";
    private readonly string _key = options.Value.ApiKey;

    public async Task OnActionExecutionAsync(ActionExecutingContext ctx, ActionExecutionDelegate next)
    {
        if (string.IsNullOrEmpty(_key)
            || !ctx.HttpContext.Request.Headers.TryGetValue(Cabecera, out var k) || k != _key)
        {
            ctx.Result = new UnauthorizedObjectResult(Respuestas.Error("API key inválida."));
            return;
        }
        await next();
    }
}
