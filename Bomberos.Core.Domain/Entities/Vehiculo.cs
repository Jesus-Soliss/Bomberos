using Bomberos.Core.Domain.Common;
using Bomberos.Core.Domain.Enums;

namespace Bomberos.Core.Domain.Entities;

public class Vehiculo : AuditableBaseEntity
{
    public int Id { get; set; }
    public string Placa { get; set; } = "";
    public TipoVehiculo Tipo { get; set; }
    public int EstacionId { get; set; }
    public Estacion Estacion { get; set; } = null!;
    public EstadoVehiculo Estado { get; set; } = EstadoVehiculo.Disponible;

    public ICollection<AsignacionVehiculo> Asignaciones { get; set; } = new List<AsignacionVehiculo>();
}
