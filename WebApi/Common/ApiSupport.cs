using Bomberos.Core.Application.Interfaces.Services;

namespace Bomberos.WebApi.Common;

/// <summary>Respuesta uniforme de error: { "success": false, "message": "..." }.</summary>
public record ApiError(string Message, IDictionary<string, string[]>? Errors = null)
{
    public bool Success => false;
}

public class CurrentUserService(IHttpContextAccessor accessor) : ICurrentUserService
{
    public string? UserId => accessor.HttpContext?.User.FindFirst("sub")?.Value;
    public string? Username => accessor.HttpContext?.User.FindFirst("name")?.Value;
}
