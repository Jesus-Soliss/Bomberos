using Bomberos.Core.Application.Interfaces.Services;
using Bomberos.Core.Domain.Entities;
using Bomberos.Core.Domain.Enums;
using Bomberos.Infrastructure.Persistence.Contexts;

namespace Bomberos.Infrastructure.Persistence.Services;

public class AuditService(ApplicationContext context, ICurrentUserService currentUser) : IAuditService
{
    public async Task RegistrarAsync(string accion, string entidad, int? entidadId, ResultadoAuditoria resultado, string? usuarioId = null)
    {
        // Una operación fallida puede dejar cambios sin guardar en el contexto; no deben viajar con el log.
        if (resultado == ResultadoAuditoria.Fallido) context.ChangeTracker.Clear();

        context.AuditLogs.Add(new AuditLog
        {
            UsuarioId = usuarioId ?? currentUser.UserId,
            Accion = accion.Length > 100 ? accion[..100] : accion,
            Entidad = entidad,
            EntidadId = entidadId,
            FechaHora = DateTime.UtcNow,
            Resultado = resultado
        });
        await context.SaveChangesAsync();
    }
}
