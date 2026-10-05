using Bomberos.Core.Domain.Enums;

namespace Bomberos.Core.Domain.Entities;

// Clave compuesta (EmergenciaId, BomberoId); se configura en Persistence.
public class AsignacionBombero
{
    public int EmergenciaId { get; set; }
    public Emergencia Emergencia { get; set; } = null!;
    public int BomberoId { get; set; }
    public Bombero Bombero { get; set; } = null!;
    public DateTime FechaAsignacion { get; set; }
    public DateTime? FechaFinalizacion { get; set; }
    public EstadoAsignacion Estado { get; set; } = EstadoAsignacion.Activa;
}
