using Bomberos.Core.Domain.Enums;

namespace Bomberos.Core.Domain.Entities;

public class AuditLog
{
    public long Id { get; set; }
    // Id del usuario de Identity, sin llave foránea (vive en otro contexto).
    public string? UsuarioId { get; set; }
    public string Accion { get; set; } = "";
    public string Entidad { get; set; } = "";
    public int? EntidadId { get; set; }
    public DateTime FechaHora { get; set; }
    public ResultadoAuditoria Resultado { get; set; }
}
