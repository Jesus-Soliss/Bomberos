using Bomberos.Core.Domain.Enums;

namespace Bomberos.Core.Domain.Entities;

// Clave compuesta (EmergenciaId, VehiculoId); se configura en Persistence.
public class AsignacionVehiculo
{
    public int EmergenciaId { get; set; }
    public Emergencia Emergencia { get; set; } = null!;
    public int VehiculoId { get; set; }
    public Vehiculo Vehiculo { get; set; } = null!;
    public DateTime FechaAsignacion { get; set; }
    public DateTime? FechaFinalizacion { get; set; }
    public EstadoAsignacion Estado { get; set; } = EstadoAsignacion.Activa;
}
