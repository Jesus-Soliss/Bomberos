using Bomberos.Core.Application.Exceptions;
using Bomberos.Core.Application.Interfaces.Services;
using Bomberos.Core.Domain.Enums;

namespace Bomberos.Core.Application.Services;

public static class AuditoriaExtensions
{
    /// <summary>
    /// Ejecuta la operación y deja el rastro en AuditLog (RN-18): EXITOSO si termina bien,
    /// FALLIDO si la rechaza una regla de negocio.
    /// </summary>
    /// <param name="idDelResultado">Para altas: obtiene el Id generado a partir del resultado.</param>
    public static async Task<T> AuditarAsync<T>(this IAuditService audit, string accion, string entidad, int? entidadId,
        Func<Task<T>> operacion, Func<T, int?>? idDelResultado = null)
    {
        T resultado;
        try
        {
            resultado = await operacion();
        }
        catch (Exception ex) when (ex is BusinessException or NotFoundException)
        {
            await audit.RegistrarAsync(accion, entidad, entidadId, ResultadoAuditoria.Fallido);
            throw;
        }

        await audit.RegistrarAsync(accion, entidad, idDelResultado?.Invoke(resultado) ?? entidadId, ResultadoAuditoria.Exitoso);
        return resultado;
    }
}
