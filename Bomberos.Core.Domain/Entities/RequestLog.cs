namespace Bomberos.Core.Domain.Entities;

public class RequestLog
{
    public long Id { get; set; }
    public DateTime FechaHora { get; set; }
    // Id del usuario de Identity, sin llave foránea (vive en otro contexto).
    public string? UsuarioId { get; set; }
    public string Metodo { get; set; } = "";
    public string Endpoint { get; set; } = "";
    public string? Peticion { get; set; }
    public int CodigoRespuesta { get; set; }
    public string? Respuesta { get; set; }
    public int DuracionMs { get; set; }
}
