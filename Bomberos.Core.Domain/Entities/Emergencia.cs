using Bomberos.Core.Domain.Common;
using Bomberos.Core.Domain.Enums;

namespace Bomberos.Core.Domain.Entities;

public class Emergencia : AuditableBaseEntity
{
    public int Id { get; set; }
    public TipoEmergencia Tipo { get; set; }
    public string Descripcion { get; set; } = "";
    public string Direccion { get; set; } = "";
    public DateTime FechaHora { get; set; }
    public PrioridadEmergencia Prioridad { get; set; }
    public EstadoEmergencia Estado { get; set; } = EstadoEmergencia.Pendiente;
    public OrigenEmergencia Origen { get; set; }
    public string? ReferenciaExterna { get; set; }

    public ICollection<AsignacionBombero> AsignacionesBomberos { get; set; } = new List<AsignacionBombero>();
    public ICollection<AsignacionVehiculo> AsignacionesVehiculos { get; set; } = new List<AsignacionVehiculo>();

    /// <summary>Ciclo de vida de la sección 5.3 (RN-11 y RN-12 quedan cubiertas por esta tabla).</summary>
    public bool PuedeCambiarA(EstadoEmergencia nuevo) => (Estado, nuevo) switch
    {
        (EstadoEmergencia.Pendiente, EstadoEmergencia.EnAtencion) => true,
        (EstadoEmergencia.Pendiente, EstadoEmergencia.Cancelada) => true,
        (EstadoEmergencia.EnAtencion, EstadoEmergencia.Controlada) => true,
        (EstadoEmergencia.Controlada, EstadoEmergencia.Finalizada) => true,
        _ => false
    };

    public bool EstaCerrada => Estado is EstadoEmergencia.Finalizada or EstadoEmergencia.Cancelada;

    public bool AdmiteAsignaciones => Estado is EstadoEmergencia.Pendiente or EstadoEmergencia.EnAtencion;
}
