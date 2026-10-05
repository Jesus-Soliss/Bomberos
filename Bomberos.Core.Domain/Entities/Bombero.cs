using Bomberos.Core.Domain.Common;
using Bomberos.Core.Domain.Enums;

namespace Bomberos.Core.Domain.Entities;

public class Bombero : AuditableBaseEntity
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string Apellido { get; set; } = "";
    public string Cedula { get; set; } = "";
    public RangoBombero Rango { get; set; }
    public int EstacionId { get; set; }
    public Estacion Estacion { get; set; } = null!;
    public EstadoBombero Estado { get; set; } = EstadoBombero.Activo;

    public ICollection<AsignacionBombero> Asignaciones { get; set; } = new List<AsignacionBombero>();
}
