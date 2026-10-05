using System.Text.Json;
using Bomberos.Core.Application.Interfaces.Services;
using Bomberos.Core.Domain.Common;

namespace Bomberos.WebApi.Common;

/// <summary>Respuesta uniforme de error: { "success": false, "message": "..." }.</summary>
public record ApiError(string Message, IDictionary<string, string[]>? Errors = null)
{
    public bool Success => false;
}

/// <summary>Los enums viajan en el JSON con el texto del documento funcional (EN_SERVICIO, CENTRAL_911, ...).</summary>
public class EnumTextoNamingPolicy : JsonNamingPolicy
{
    public override string ConvertName(string name) => EnumTexto.Convertir(name);
}

public class CurrentUserService(IHttpContextAccessor accessor) : ICurrentUserService
{
    public string? UserId => accessor.HttpContext?.User.FindFirst("sub")?.Value;
    public string? Username => accessor.HttpContext?.User.FindFirst("name")?.Value;
}
