using Bomberos.Core.Application.DTOs.Emergencias;
using Bomberos.Core.Domain.Enums;

namespace Bomberos.Core.Application.DTOs.Consultas;

public class DashboardDto
{
    public int TotalEmergencias { get; set; }
    public int EmergenciasHoy { get; set; }
    // Clave: texto del estado u origen (PENDIENTE, CENTRAL_911, ...). Incluye los que están en cero.
    public Dictionary<string, int> EmergenciasPorEstado { get; set; } = [];
    public Dictionary<string, int> EmergenciasPorOrigen { get; set; } = [];
    public int BomberosDisponibles { get; set; }
    public int BomberosEnServicio { get; set; }
    public int VehiculosDisponibles { get; set; }
    public int VehiculosEnServicio { get; set; }
    public int VehiculosEnMantenimiento { get; set; }
    public List<EmergenciaDto> UltimasEmergencias { get; set; } = [];
}

public class LogFiltroDto
{
    public string? UsuarioId { get; set; }
    /// <summary>Acción (AuditLog) o parte del endpoint (RequestLog).</summary>
    public string? Accion { get; set; }
    public DateTime? Desde { get; set; }
    public DateTime? Hasta { get; set; }
    public int Limite { get; set; } = 200;
}

public class AuditLogDto
{
    public long Id { get; set; }
    public string? UsuarioId { get; set; }
    public string Accion { get; set; } = "";
    public string Entidad { get; set; } = "";
    public int? EntidadId { get; set; }
    public DateTime FechaHora { get; set; }
    public ResultadoAuditoria Resultado { get; set; }
}

public class RequestLogDto
{
    public long Id { get; set; }
    public DateTime FechaHora { get; set; }
    public string? UsuarioId { get; set; }
    public string Metodo { get; set; } = "";
    public string Endpoint { get; set; } = "";
    public string? Peticion { get; set; }
    public int CodigoRespuesta { get; set; }
    public string? Respuesta { get; set; }
    public int DuracionMs { get; set; }
}
